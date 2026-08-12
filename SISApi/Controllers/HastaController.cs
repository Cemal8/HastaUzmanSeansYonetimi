using Microsoft.AspNetCore.Mvc;
using System;

using IS = SISIsKatmani;
using VAR = SISVarliklar;

namespace SISApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HastaController : ControllerBase
    {
        [HttpPost("kaydet")]
        public IActionResult Kaydet([FromBody] VAR.Hasta hasta)
        {
            try
            {
                int sonuc = IS.Hasta.Kaydet(hasta);
                return Ok(sonuc); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("listele")]
        public IActionResult HastalariListele([FromQuery] string? ad, [FromQuery] string? soyad)
        {
            try
            {
                var hastalar = IS.Hasta.HastalariListele(ad ?? string.Empty, soyad ?? string.Empty);
                return Ok(hastalar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{hastaNo}")]
        public IActionResult HastaGetir(int hastaNo)
        {
            try
            {
                var hasta = IS.Hasta.HastaGetir(hastaNo);

                if (hasta == null)
                    return NotFound(); 

                return Ok(hasta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}