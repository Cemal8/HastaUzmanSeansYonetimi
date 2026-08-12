using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAR = SISVarliklar;

namespace SISWin
{
    public partial class FormParolaDegistir : Form
    {
        // API Adresimiz
        private readonly string apiUrl = "https://localhost:7003/";

        public VAR.Calisan calisan;

        public FormParolaDegistir()
        {
            InitializeComponent();
        }

        private bool KullanıcıGirdisiDogrula()
        {
            if (txtEski.Text == String.Empty)
            {
                MessageBox.Show("Eski Parola alanı boş geçilemez.");
                txtEski.Select();
                txtEski.Focus();
                return false;
            }
            if (txtYeni.Text == String.Empty)
            {
                MessageBox.Show("Yeni Parola alanı boş geçilemez.");
                txtYeni.Select();
                txtYeni.Focus();
                return false;
            }

            if (txtYeni.Text != txtYeni2.Text)
            {
                MessageBox.Show("Yeni Parola ve Yeni Parola Tekrar alanları aynı olmalıdır.");
                txtYeni2.Select();
                txtYeni2.Focus();
                return false;
            }
            return true;
        }

        private async void btnDegistir_Click(object sender, EventArgs e)
        {
            bool dogruMu = KullanıcıGirdisiDogrula();
            if (!dogruMu)
            {
                return;
            }

            btnDegistir.Enabled = false;
            bool sonuc = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var requestData = new
                    {
                        CalisanNo = calisan.No,
                        YeniParola = txtYeni.Text
                    };

                    HttpResponseMessage response = await client.PostAsJsonAsync("api/calisan/parola-degistir", requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        sonuc = await response.Content.ReadFromJsonAsync<bool>();
                    }
                    else
                    {
                        MessageBox.Show($"API İşlemi Başarısız Oldu. Statü Kodu: {response.StatusCode}");
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
                btnDegistir.Enabled = true;
            }

            if (sonuc)
            {
                MessageBox.Show("Parola başarıyla değiştirildi.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Parola değiştirilirken bir hata oluştu.");
            }
        }
    }
}