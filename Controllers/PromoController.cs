using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.AffiliateRepositories;
using ProjectAgaman.Repositories.PromoRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromoController : ControllerBase
    {
        private readonly IPromoRepository _promoRepository;
        private readonly IConfiguration _configuration;
        public PromoController(IPromoRepository promoRepository, IConfiguration configuration)
        {
            _promoRepository = promoRepository;
            _configuration = configuration;
        }

        [HttpGet("GetAllPromos")]
        public async Task<ActionResult<IEnumerable<PromoDTO>>> GetAllPromos()
        {
            try
            {
                var promos = await _promoRepository.GetAllPromosAsync();
                return Ok(promos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetPromoById")]
        public async Task<ActionResult<PromoDTO>> GetPromoById(int id)
        {
            try
            {
                var promo = await _promoRepository.GetPromoByIdAsync(id);
                if (promo == null)
                    return NotFound($"Promo with ID {id} not found.");
                return Ok(promo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("GetPromoByCode")]
        public async Task<ActionResult<PromoResponseDTO>> GetPromoByCode(string promoCode)
        {
            try
            {
                var promo = await _promoRepository.GetPromoByCodeAsync(promoCode);

                return Ok(promo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("AddPromo")]
        public async Task<ActionResult<ResponseDTO>> AddPromo([FromBody] PromoDTO promo)
        {
            if (promo == null)
                return BadRequest("Invalid promo data.");
            try
            {
                var response = await _promoRepository.AddPromoAsync(promo);
                if (response.IsSuccess)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("UpdatePromo")]
        public async Task<ActionResult<ResponseDTO>> UpdatePromo([FromBody] PromoDTO promo)
        {
            if (promo == null || promo.PkPromoId <= 0)
                return BadRequest("Invalid promo data.");
            try
            {
                var response = await _promoRepository.UpdatePromoAsync(promo);
                if (response.IsSuccess)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("DeletePromo")]
        public async Task<ActionResult<ResponseDTO>> DeletePromo(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid promo ID.");
            try
            {
                var response = await _promoRepository.DeletePromoAsync(id);
                if (response.IsSuccess)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("ActivatePromo")]
        public async Task<ActionResult<ResponseDTO>> ActivatePromo(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid promo ID.");
            try
            {
                var response = await _promoRepository.ActivatePromoAsync(id);
                if (response.IsSuccess)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

    }
}
