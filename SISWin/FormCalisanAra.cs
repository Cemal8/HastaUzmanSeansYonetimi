using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAR = SISVarliklar;

namespace SISWin
{
    public partial class FormCalisanAra : Form
    {
        // API Adresimiz
        private readonly string apiUrl = "https://localhost:7003/";

        public VAR.Calisan calisan;

        public FormCalisanAra()
        {
            InitializeComponent();
            lstCalisanlar.DisplayMember = "GoruntuMetni";
        }

        private async Task CalisanlariListeleAsync()
        {
            lstCalisanlar.DataSource = null;

            string ad = txtAd.Text.Trim();
            string soyad = txtSoyad.Text.Trim();

            string requestUrl = $"api/calisan/listele?ad={ad}&soyad={soyad}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var calisanlar = await client.GetFromJsonAsync<VAR.Calisan[]>(requestUrl);

                    lstCalisanlar.DataSource = calisanlar;
                    lstCalisanlar.DisplayMember = "GoruntuMetni";
                }
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                MessageBox.Show("Servisten veri çekilirken bir hata oluştu!\nDetay: " + ex.Message);
            }
        }

        private async void btnAra_Click_1(object sender, EventArgs e)
        {
            btnAra.Enabled = false;
            await CalisanlariListeleAsync();
            btnAra.Enabled = true;
        }

        private async void lstCalisanlar_DoubleClick(object sender, EventArgs e)
        {
            if (lstCalisanlar.SelectedItem == null) return;

            int indeks = lstCalisanlar.SelectedIndex;
            VAR.Calisan secilenCalisan = (VAR.Calisan)lstCalisanlar.SelectedItem;

            if (secilenCalisan.CalisanTipi == VAR.CalisanTipleri.Sekreter)
            {
                FormSekreterBilgisi frm = new FormSekreterBilgisi();
                frm.calisan = secilenCalisan;
                frm.ShowDialog();
            }
            else if (secilenCalisan.CalisanTipi == VAR.CalisanTipleri.Uzman)
            {
                FormUzmanBilgisi frm = new FormUzmanBilgisi();
                frm.calisan = secilenCalisan;
                frm.ShowDialog();
            }

            await CalisanlariListeleAsync();

            if (lstCalisanlar.Items.Count > 0 && indeks < lstCalisanlar.Items.Count)
            {
                lstCalisanlar.SelectedIndex = indeks;
            }
        }
    }
}