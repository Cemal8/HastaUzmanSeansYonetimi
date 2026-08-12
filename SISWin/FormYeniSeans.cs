using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAR = SISVarliklar;

namespace SISWin
{
    public partial class FormYeniSeans : Form
    {
        // API Adresimiz
        private readonly string apiUrl = "https://localhost:7003/";

        public VAR.Calisan uzman;
        private const int enKisaSeansSuresi = 30;

        public FormYeniSeans(VAR.Calisan seciliUzman)
        {
            InitializeComponent();
            uzman = seciliUzman;
            lblUzman.Text = uzman.GoruntuMetni;
        }

        private DateTime SeansBaslangicZamaniGetir()
        {
            return dtpTarih.Value.Date
                .AddHours(dtpBaslingicSaati.Value.Hour)
                .AddMinutes(dtpBaslingicSaati.Value.Minute);
        }

        private DateTime SeansBitisZamaniGetir()
        {
            DateTime baslangic = SeansBaslangicZamaniGetir();
            DateTime bitis = dtpTarih.Value.Date
                .AddHours(dtpBitisSaati.Value.Hour)
                .AddMinutes(dtpBitisSaati.Value.Minute);

            if (bitis <= baslangic)
            {
                bitis = bitis.AddDays(1);
            }

            return bitis;
        }

        private bool KullanıcıGirdisiDogrula()
        {
            if (dtpTarih.Value.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Seans tarihi bugünden önce olamaz.");
                dtpTarih.Select();
                dtpTarih.Focus();
                return false;
            }

            DateTime baslangic = SeansBaslangicZamaniGetir();
            DateTime bitis = SeansBitisZamaniGetir();

            if ((bitis - baslangic).TotalMinutes < enKisaSeansSuresi)
            {
                MessageBox.Show($"Seans süresi en az {enKisaSeansSuresi} dakika olmalıdır.");
                dtpBitisSaati.Select();
                dtpBitisSaati.Focus();
                return false;
            }
            return true;
        }

        private async void btnKaydet_Click(object sender, EventArgs e)
        {
            if (uzman == null)
            {
                MessageBox.Show("Uzman bilgisi bulunamadı!");
                return;
            }

            bool dogruMu = KullanıcıGirdisiDogrula();

            if (!dogruMu)
            {
                return;
            }

            VAR.Seans seans = new VAR.Seans();
            seans.UzmanNo = uzman.No;
            seans.Tarih = dtpTarih.Value;
            seans.BaslangicSaati = dtpBaslingicSaati.Value.ToShortTimeString();
            seans.BitisSaati = dtpBitisSaati.Value.ToShortTimeString();

            int sonuc = 0;

            btnKaydet.Enabled = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    HttpResponseMessage response = await client.PostAsJsonAsync("api/seans/kaydet", seans);

                    if (response.IsSuccessStatusCode)
                    {
                        sonuc = await response.Content.ReadFromJsonAsync<int>();
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
                MessageBox.Show("Servise bağlanırken bir hata oluştu!\nDetay: " + ex.Message);
            }
            finally
            {
                btnKaydet.Enabled = true;
            }

            if (sonuc > 0)
            {
                MessageBox.Show("Kayıt işlemi başarıyla tamamlandı.");
                this.Close(); 
            }
            else if (sonuc == -1)
            {
                MessageBox.Show("Seans saatlerinde çakışma var! Lütfen başka bir saat dilimi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("İşlem hatalı veya gerçekleştirilemedi.");
            }
        }

        private void FormYeniSeans_Load(object sender, EventArgs e)
        {

        }
    }
}