using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAR = SISVarliklar;

namespace SISWin
{
    public partial class FormHastaRandevuYonet : Form
    {
        // API Adresimiz
        private readonly string apiUrl = "https://localhost:7003/";

        public VAR.Hasta hasta = null;
        private VAR.Seans sonRandevu = null;
        private VAR.Seans sonSeans = null;

        public FormHastaRandevuYonet()
        {
            InitializeComponent();
        }

        private void FormHastaRandevuYonet_Load(object sender, EventArgs e)
        {
            lstHastalar.DisplayMember = "GoruntuMetni";
            HastaBilgisiTemizle();
            RandevuBilgisiTemizle();
        }

        private async Task HastalariListeleAsync()
        {
            lstHastalar.DataSource = null;

            string ad = txtAd.Text.Trim();
            string soyad = txtSoyad.Text.Trim();
            string requestUrl = $"api/hasta/listele?ad={ad}&soyad={soyad}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var hastalar = await client.GetFromJsonAsync<VAR.Hasta[]>(requestUrl);
                    lstHastalar.DataSource = hastalar;
                    lstHastalar.DisplayMember = "GoruntuMetni";
                }
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                MessageBox.Show("Servisten hasta listesi çekilirken bir hata oluştu!\nDetay: " + ex.Message);
            }
        }

        private async Task RandevuBilgisiYukleAsync()
        {
            RandevuBilgisiTemizle();

            if (hasta == null) return;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    HttpResponseMessage randevuResponse = await client.GetAsync($"api/seans/hasta/{hasta.No}/son-randevu");

                    if (randevuResponse.IsSuccessStatusCode)
                    {
                        sonRandevu = await randevuResponse.Content.ReadFromJsonAsync<VAR.Seans>();
                    }
                    else
                    {
                        sonRandevu = null;
                    }

                    if (sonRandevu != null)
                    {
                        lblRandevuSeans.Text = sonRandevu.GoruntuMetni;
                        lblRandevuUzman.Text = sonRandevu.UzmanBilgisi;
                        lnkRandevuIptalEt.Enabled = true;
                    }
                    else
                    {
                        lnkYeniRandevu.Enabled = true;
                    }

                    HttpResponseMessage seansResponse = await client.GetAsync($"api/seans/hasta/{hasta.No}/son-seans");

                    if (seansResponse.IsSuccessStatusCode)
                    {
                        sonSeans = await seansResponse.Content.ReadFromJsonAsync<VAR.Seans>();
                    }
                    else
                    {
                        sonSeans = null;
                    }

                    if (sonSeans != null)
                    {
                        lblSeansUzman.Text = sonSeans.UzmanBilgisi;
                        lblSeansSeans.Text = sonSeans.GoruntuMetni;
                        lblSeansNot.Text = sonSeans.SeansNotu;
                    }
                }
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                MessageBox.Show("Servisten seans bilgileri çekilirken hata oluştu!");
            }
        }

        private void HastalariYukle()
        {
            if (hasta == null) return;

            HastaBilgisiTemizle();
            lblAd.Text = hasta.Ad;
            lblSoyad.Text = hasta.Soyad;
            lblGsmNo.Text = hasta.CepTel;
            lblCinsiyet.Text = hasta.Cinsiyet;
            lblDogumTarihi.Text = hasta.DogumTarihi.ToShortDateString();
            lblEPosta.Text = hasta.Eposta;
            lblTelefon.Text = hasta.EvTel;
            lblTCKimlikNo.Text = hasta.TcKimlikNo;
            lblAdres.Text = hasta.Adres;
        }

        private void HastaBilgisiTemizle()
        {
            lblAd.Text = "";
            lblSoyad.Text = "";
            lblGsmNo.Text = "";
            lblCinsiyet.Text = "";
            lblDogumTarihi.Text = "";
            lblEPosta.Text = "";
            lblTelefon.Text = "";
            lblTCKimlikNo.Text = "";
            lblAdres.Text = "";
        }

        private void RandevuBilgisiTemizle()
        {
            lblRandevuSeans.Text = "";
            lblRandevuUzman.Text = "";
            lblSeansNot.Text = "";
            lblSeansSeans.Text = "";
            lblSeansUzman.Text = "";
            lnkYeniRandevu.Enabled = false;
            lnkRandevuIptalEt.Enabled = false;
        }


        private async void btnAra_Click(object sender, EventArgs e)
        {
            btnAra.Enabled = false;
            await HastalariListeleAsync();
            btnAra.Enabled = true;
        }

        private async void lstHastalar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstHastalar.SelectedIndex < 0) return;

            hasta = (VAR.Hasta)lstHastalar.Items[lstHastalar.SelectedIndex];
            HastalariYukle();
            await RandevuBilgisiYukleAsync();
        }

        private async void lnkYeniHastaGir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormHastaBilgisi frm = new FormHastaBilgisi();
            DialogResult cevap = frm.ShowDialog();

            if (cevap == DialogResult.OK || frm.DialogResult == DialogResult.OK)
            {
                txtAd.Text = frm.hasta?.Ad ?? txtAd.Text;
                txtSoyad.Text = frm.hasta?.Soyad ?? txtSoyad.Text;

                await HastalariListeleAsync();

                if (lstHastalar.Items.Count > 0)
                {
                    lstHastalar.SelectedIndex = 0;
                }
            }
        }

        private async void lnkHastaGuncelle_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (hasta == null) return;

            int indeks = lstHastalar.SelectedIndex;
            FormHastaBilgisi frm = new FormHastaBilgisi();
            frm.hasta = this.hasta;
            frm.ShowDialog();

            await HastalariListeleAsync();

            if (lstHastalar.Items.Count > 0 && indeks < lstHastalar.Items.Count)
            {
                lstHastalar.SelectedIndex = indeks;
            }
        }

        private async void lnkYeniRandevu_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (hasta == null) return;

            FormRandevuKaydet frm = new FormRandevuKaydet();
            frm.hasta = this.hasta;
            frm.ShowDialog();

            await RandevuBilgisiYukleAsync();
        }

        private async void lnkRandevuIptalEt_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sonRandevu == null) return;

            string mesaj = "Randevuyu iptal etmek istediğinize emin misiniz?";
            DialogResult karar = MessageBox.Show(mesaj, "İptal Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (karar == DialogResult.Yes)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(apiUrl);

                        HttpResponseMessage response = await client.PostAsync($"api/seans/randevu-iptal/{sonRandevu.No}", null);

                        if (response.IsSuccessStatusCode)
                        {
                            bool sonuc = await response.Content.ReadFromJsonAsync<bool>();

                            if (sonuc)
                            {
                                MessageBox.Show("Randevu başarıyla iptal edildi.");
                                await RandevuBilgisiYukleAsync(); // Ekranı yenile
                            }
                            else
                            {
                                MessageBox.Show("Randevu iptal edilemedi, işlem başarısız oldu.");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"API Hatası: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Yardimci.HataKaydet(ex);
                    MessageBox.Show("Serviste bir hata oluştu!");
                }
            }
        }

        private void grbHastaBilgileri_Enter(object sender, EventArgs e)
        {
        }
    }
}