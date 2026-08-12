using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAR = SISVarliklar;

namespace SISVeriErisimKatmani
{
    public class Calisan
    {

        public static int KullaniciGirisiniDogrula(string ePosta, string sifre)
        {
            string spAdi = "prCalisanGirisiniDogrula";
            Komut k = new Komut(spAdi);
            k.ParametreEkle("@EPosta", ePosta);
            k.ParametreEkle("@parola", sifre);

            int sonuc = k.IsletInt32();
            k.Temizle();
            return sonuc;
        }

        public static SISVarliklar.Calisan CalisanGetir(int calisanNo)
        {
            string spAdi = "prCalisanGetir";
            VAR.Calisan calisan = null;

            Komut k = new Komut(spAdi);
            k.ParametreEkle("@CalisanNo", calisanNo);
            SqlDataReader sdr = k.IsletReader();
            while (sdr.Read())
            {
                calisan = CalisanBilgileriYukle(sdr);
            }
            sdr.Close();
            k.Temizle();

            return calisan;
        }

        private static VAR.Calisan CalisanBilgileriYukle(SqlDataReader sdr)
        {
            VAR.Calisan calisan = new VAR.Calisan();
            try
            {
                calisan.No = Komut.Int32Getir(sdr, "CalisanNo");
                calisan.Ad = Komut.StringGetir(sdr, "Ad");
                calisan.CepTel = Komut.StringGetir(sdr, "CepTel");
                calisan.Eposta = Komut.StringGetir(sdr, "Eposta");
                calisan.EvTel = Komut.StringGetir(sdr, "EvTel");
                calisan.Soyad = Komut.StringGetir(sdr, "Soyad");
                calisan.TcKimlikNo = Komut.StringGetir(sdr, "TcKimlikNo");
                calisan.Unvan = Komut.StringGetir(sdr, "Unvan");
                calisan.Parola = Komut.StringGetir(sdr, "Parola");

                int calisanTipi = Komut.Int16Getir(sdr, "CalisanTipi");
                calisan.CalisanTipi = (VAR.CalisanTipleri)calisanTipi;
            }
            catch (Exception ex)
            {
                Yardimci.HataKaydet(ex);
                calisan = null;
            }

            return calisan;

        }

        public static bool Kaydet (VAR.Calisan calisan)
        {
            string spAdi = "prCalisanYeniKaydet";
            int etkilenen = 0;

            Komut k = new Komut(spAdi);
            k.ParametreEkle("@tcKimlikNo", calisan.TcKimlikNo);
            k.ParametreEkle("@ad", calisan.Ad);
            k.ParametreEkle("@soyad", calisan.Soyad);
            k.ParametreEkle("@unvan", calisan.Unvan);
            k.ParametreEkle("@ePosta", calisan.Eposta);
            k.ParametreEkle("@ceptel", calisan.CepTel);
            k.ParametreEkle("@evTel", calisan.EvTel);
            k.ParametreEkle("@parola", calisan.Parola);
            k.ParametreEkle("@calisanTipi", calisan.CalisanTipi);
            k.ParametreEkleOut("@calisanNo", SqlDbType.Int, 0);

            etkilenen = k.Islet();
            if (etkilenen > 0)
            {
                calisan.No = k.OutParametreDegeriGetir("@calisanNo");
            }

            k.Temizle();
            return etkilenen > 0;

        }


        public static bool Guncelle(SISVarliklar.Calisan calisan)
        {
            string spAdi = "prCalisanGuncelle";
            int etkilenen = 0;

            Komut k = new Komut(spAdi);
            k.ParametreEkle("@calisanNo", calisan.No);
            k.ParametreEkle("@tCKimlikNo", calisan.TcKimlikNo);
            k.ParametreEkle("@ad", calisan.Ad);
            k.ParametreEkle("@soyad", calisan.Soyad);
            k.ParametreEkle("@unvan", calisan.Unvan);
            k.ParametreEkle("@ePosta", calisan.Eposta);
            k.ParametreEkle("@cepTel", calisan.CepTel);
            k.ParametreEkle("@evTel", calisan.EvTel);
            k.ParametreEkle("@parola", calisan.Parola);
            k.ParametreEkle("@calisanTipi", calisan.CalisanTipi);

            etkilenen = k.Islet();
            k.Temizle();
            return etkilenen > 0;

        }

        public static SISVarliklar.Calisan[] CalisanlariListele (string ad, string soyad)
        {
            string spAdi = "prCalisanlariListele";
            List<VAR.Calisan> calisanlar=new List<VAR.Calisan>();

            Komut k = new Komut(spAdi);
            k.ParametreEkle("@ad", ad ?? string.Empty);
            k.ParametreEkle("@soyad", soyad ?? string.Empty);

            SqlDataReader sdr = k.IsletReader();
            while (sdr.Read())
            {
                VAR.Calisan calisan = CalisanBilgileriYukle(sdr);
                calisanlar.Add(calisan);
            }
            sdr.Close();
            k.Temizle();

            return calisanlar.ToArray();
        }

        public static SISVarliklar.Calisan[] UzmanlariListele()
        {
            string spAdi = "prCalisanUzmanlariListele";
            List<VAR.Calisan> calisanlar = new List<SISVarliklar.Calisan>();

            Komut k = new Komut(spAdi);

            SqlDataReader sdr = k.IsletReader();
            while (sdr.Read())
            {
                VAR.Calisan calisan = CalisanBilgileriYukle(sdr);
                calisanlar.Add(calisan);
            }

            sdr.Close();
            k.Temizle();

            return calisanlar.ToArray();
        }

        public static bool ParolaDegistir(int calisanNo, string yeniParola)
        {
            string spAdi = "prCalisanParolaDegistir";

            Komut k = new Komut(spAdi);
            k.ParametreEkle("@calisanNo", calisanNo);
            k.ParametreEkle("@parola", yeniParola);
            int etkilenen = k.Islet();
            k.Temizle();

            return etkilenen > 0;
        }

        public static bool ParolaSifirla(string ePosta, string parola)
        {
            string spAdi = "prCalisanParolaSifirla";

            Komut k = new Komut(spAdi);
            k.ParametreEkle("@ePosta", ePosta);
            k.ParametreEkle("@parola", parola);
            int etkilenen = k.Islet();
            k.Temizle();

            return etkilenen > 0;
        }
    }
}
