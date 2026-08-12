using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAR = SISVarliklar;

namespace SISWin
{
    public partial class FormUzmanSeansYonetimi : Form
    {
        // API Adresimiz
        private readonly string apiUrl = "https://localhost:7003/";

        public VAR.Calisan uzman = null;
        private VAR.Hasta hasta = null;
        private VAR.Seans aktifseans = null;
        private VAR.Seans gecmisSeans = null;

        public FormUzmanSeansYonetimi()
        {
            InitializeComponent();
        }

        private void EkranıTemizle()
        {
            hasta = null;
            lblTCKimlikNo.Text = "";
            lblAd.Text = "";
            lblAdres.Text = "";
            lblGsmNo.Text = "";
            lblCinsiyet.Text = "";
            lblDogumTarihi.Text = "";
            lblEPosta.Text = "";
            lblTelefon.Text = "";
            lblSoyad.Text = "";
            txtAktifSeansNotu.Text = "";
            lstSeanslar.DataSource = null;
        }

        private async void FormUzmanSeansYonetimi_Load(object sender, EventArgs e)
        {
            EkranıTemizle();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    uzman = await client.GetFromJsonAsync<VAR.Calisan>($"api/calisan/{Yardimci.KullaniciNo}");
                }
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                MessageBox.Show("Uzman bilgileri çekilirken serviste bir hata oluştu!");
                return;
            }

            await UzmanSeanslariniYukleAsync();
        }

        private async Task UzmanSeanslariniYukleAsync()
        {
            if (uzman == null) return;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var seanslar = await client.GetFromJsonAsync<VAR.Seans[]>($"api/seans/uzman/{uzman.No}");

                    cbbSeanslar.DataSource = seanslar;
                    cbbSeanslar.DisplayMember = "GoruntuMetni";
                }
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                MessageBox.Show("Seanslar çekilirken serviste bir hata oluştu!");
            }
        }

        private async void cbbSeanslar_SelectedIndexChanged(object sender, EventArgs e)
        {
            EkranıTemizle();

            if (cbbSeanslar.SelectedItem is VAR.Seans secilenSeans)
            {
                aktifseans = secilenSeans;

                if (aktifseans.HastaNo > 0)
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(apiUrl);
                            hasta = await client.GetFromJsonAsync<VAR.Hasta>($"api/hasta/{aktifseans.HastaNo}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Yardimci.HataKaydet(ex);
                        MessageBox.Show("Hasta bilgisi çekilirken serviste bir hata oluştu!");
                    }
                }

                await BilgileriYukleAsync();
            }
        }

        private async Task BilgileriYukleAsync()
        {
            if (aktifseans != null)
            {
                txtAktifSeansNotu.Text = aktifseans.SeansNotu ?? "";
            }

            if (hasta == null) return;

            lblAd.Text = hasta.Ad;
            lblAdres.Text = hasta.Adres;
            lblGsmNo.Text = hasta.CepTel;
            lblCinsiyet.Text = hasta.Cinsiyet;
            lblDogumTarihi.Text = hasta.DogumTarihi.ToShortDateString();
            lblEPosta.Text = hasta.Eposta;
            lblTelefon.Text = hasta.EvTel;
            lblSoyad.Text = hasta.Soyad;
            lblTCKimlikNo.Text = hasta.TcKimlikNo;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var hastaSeanslari = await client.GetFromJsonAsync<VAR.Seans[]>($"api/seans/hasta/{hasta.No}/seanslar");

                    lstSeanslar.DataSource = hastaSeanslari;
                    lstSeanslar.DisplayMember = "GoruntuMetni";
                }
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                MessageBox.Show("Geçmiş seanslar çekilirken serviste bir hata oluştu!");
            }
        }

        private void lstSeanslar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSeanslar.SelectedItem is VAR.Seans secilenGecmisSeans)
            {
                gecmisSeans = secilenGecmisSeans;
                txtGecmisSeansNotu.Text = gecmisSeans.SeansNotu;
            }
        }

        private async Task<bool> SeansNotuKaydetAsync(int seansNo, string seansNotu)
        {
            bool sonuc = false;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var requestData = new { SeansNo = seansNo, SeansNotu = seansNotu };

                    HttpResponseMessage response = await client.PutAsJsonAsync("api/seans/not-guncelle", requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        sonuc = await response.Content.ReadFromJsonAsync<bool>();
                    }
                }
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                MessageBox.Show("Not güncellenirken serviste bir hata oluştu!");
            }
            return sonuc;
        }

        private void SeansSec(int seansNo)
        {
            for (int i = 0; i < cbbSeanslar.Items.Count; i++)
            {
                if (cbbSeanslar.Items[i] is VAR.Seans seans && seans.No == seansNo)
                {
                    cbbSeanslar.SelectedIndex = i;
                    return;
                }
            }
        }

        private async void btnKaydet_Click(object sender, EventArgs e)
        {
            if (aktifseans == null) return;

            if (string.IsNullOrEmpty(txtAktifSeansNotu.Text.Trim()))
            {
                MessageBox.Show("Seans notu boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAktifSeansNotu.Focus();
                return;
            }

            btnKaydet.Enabled = false;

            int seciliSeansNo = aktifseans.No;
            string kaydedilenNot = txtAktifSeansNotu.Text.Trim();
            bool sonuc = await SeansNotuKaydetAsync(seciliSeansNo, kaydedilenNot);

            btnKaydet.Enabled = true;

            if (sonuc)
            {
                aktifseans.SeansNotu = kaydedilenNot;
                await UzmanSeanslariniYukleAsync();
                SeansSec(seciliSeansNo);
                txtAktifSeansNotu.Text = kaydedilenNot;
                MessageBox.Show("Seans notu başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Seans notu kaydedilemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGüncelle_Click(object sender, EventArgs e)
        {
            if (gecmisSeans == null)
            {
                MessageBox.Show("Lütfen güncellemek için geçmiş bir seans seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtGecmisSeansNotu.Text.Trim()))
            {
                MessageBox.Show("Seans notu boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGecmisSeansNotu.Focus();
                return;
            }

            btnGüncelle.Enabled = false;

            int seciliSeansNo = gecmisSeans.No;
            string guncellenenNot = txtGecmisSeansNotu.Text.Trim();
            bool sonuc = await SeansNotuKaydetAsync(seciliSeansNo, guncellenenNot);

            btnGüncelle.Enabled = true;

            if (sonuc)
            {
                gecmisSeans.SeansNotu = guncellenenNot;
                await BilgileriYukleAsync();

                for (int i = 0; i < lstSeanslar.Items.Count; i++)
                {
                    if (lstSeanslar.Items[i] is VAR.Seans seans && seans.No == seciliSeansNo)
                    {
                        lstSeanslar.SelectedIndex = i;
                        break;
                    }
                }

                txtGecmisSeansNotu.Text = guncellenenNot;
                MessageBox.Show("Seans notu başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Seans notu güncellenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtGecmisSeansNotu_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}