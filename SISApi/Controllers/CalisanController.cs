using Microsoft.AspNetCore.Mvc;
using System;
using IS = SISIsKatmani;
using VAR = SISVarliklar;

namespace SISApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalisanController : ControllerBase
    {
        [HttpPost("giris")]
        public IActionResult KullaniciGirisiniDogrula([FromBody] LoginRequest request)
        {
            try
            {
                int sonuc = IS.Calisan.KullaniciGirisiniDogrula(request.EPosta, request.Parola);

                if (sonuc > 0)
                    return Ok(sonuc); 

                return Unauthorized("Giriş başarısız. E-posta veya şifre hatalı."); // 401 Hata kodu
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{calisanNo}")]
        public IActionResult CalisanGetir(int calisanNo)
        {
            try
            {
                var calisan = IS.Calisan.CalisanGetir(calisanNo);

                if (calisan == null)
                    return NotFound("Çalışan bulunamadı."); 

                return Ok(calisan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("parola-degistir")]
        public IActionResult ParolaDegistir([FromBody] ParolaDegistirRequest request)
        {
            try
            {
                bool sonuc = IS.Calisan.ParolaDegistir(request.CalisanNo, request.YeniParola);
                return Ok(sonuc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("parola-sifirla")]
        public IActionResult ParolaSifirla([FromBody] LoginRequest request)
        {
            try
            {
                bool sonuc = IS.Calisan.ParolaSifirla(request.EPosta, request.Parola);
                return Ok(sonuc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("kaydet")]
        public IActionResult Kaydet([FromBody] VAR.Calisan calisan)
        {
            try
            {
                int sonuc = IS.Calisan.Kaydet(calisan);
                return Ok(sonuc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("listele")]
        public IActionResult CalisanlariListele([FromQuery] string? ad, [FromQuery] string? soyad)
        {
            try
            {
                var calisanlar = IS.Calisan.CalisanlariListele(ad ?? string.Empty, soyad ?? string.Empty);
                return Ok(calisanlar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("uzmanlar")]
        public IActionResult UzmanlariListele()
        {
            try
            {
                var uzmanlar = IS.Calisan.UzmanlariListele();
                return Ok(uzmanlar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    // --- YARDIMCI SINIFLAR (DTO - Data Transfer Objects) ---
    // Bu sınıfları POST işlemlerinde birden fazla veriyi tek bir Body objesi olarak almak için kullanıyoruz.
    // İstersen bu sınıfları projenin içinde "Models" veya "DTOs" adında ayrı bir klasöre de taşıyabilirsin.

    public class LoginRequest
    {
        public string EPosta { get; set; } = string.Empty;
        public string Parola { get; set; } = string.Empty;
    }

    public class ParolaDegistirRequest
    {
        public int CalisanNo { get; set; }
        public string YeniParola { get; set; } = string.Empty;
    }
}