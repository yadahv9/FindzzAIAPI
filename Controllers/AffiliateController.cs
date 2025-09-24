using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Core.Helpers;
using ProjectAgaman.Repositories.AffiliateRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffiliateController : ControllerBase
    {
        private readonly IAffiliateRepository _affiliateRepository;
        private readonly IConfiguration _configuration;
        public AffiliateController(IAffiliateRepository affiliateRepository, IConfiguration configuration)
        {
            _affiliateRepository = affiliateRepository;
            _configuration = configuration;
        }

        [HttpGet("GetAllAffiliates")]
        public async Task<ActionResult<IEnumerable<AffiliateDTO>>> GetAllAffiliates()
        {
            try
            {
                var affiliates = await _affiliateRepository.GetAllAffiliatesAsync();
                return Ok(affiliates);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAffiliateById")]
        public async Task<ActionResult<AffiliateDTO>> GetAffiliateById(int id)
        {
            try
            {
                var affiliate = await _affiliateRepository.GetAffiliateByIdAsync(id);
                if (affiliate == null)
                    return NotFound($"Affiliate with ID {id} not found.");
                affiliate.Password = EncryptionHelper.DecryptString(_configuration["EncryptionKey"], affiliate.Password);
                return Ok(affiliate);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddAffiliate")]
        public async Task<ActionResult<ResponseDTO>> AddAffiliate([FromBody] AffiliateDTO affiliate)
        {
            if (affiliate == null)
                return BadRequest("Invalid affiliate data.");
            try
            {
                var response = await _affiliateRepository.AddAffiliateAsync(affiliate);
                if (!response.IsSuccess)
                    return BadRequest(response.ErrorMessage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateAffiliate")]
        public async Task<ActionResult<ResponseDTO>> UpdateAffiliate([FromBody] AffiliateDTO affiliate)
        {
            if (affiliate == null)
                return BadRequest("Invalid affiliate data.");
            try
            {
                var response = await _affiliateRepository.UpdateAffiliateAsync(affiliate);
                if (!response.IsSuccess)
                    return BadRequest(response.ErrorMessage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteAffiliate")]
        public async Task<ActionResult<ResponseDTO>> DeleteAffiliate(int id)
        {
            try
            {
                var response = await _affiliateRepository.DeleteAffiliateAsync(id);
                if (!response.IsSuccess)
                    return BadRequest(response.ErrorMessage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("ActivateAffiliate")]
        public async Task<ActionResult<ResponseDTO>> ActivateAffiliate(int id)
        {
            try
            {
                var affiliate = await _affiliateRepository.GetAffiliateByIdAsync(id);
                if (affiliate == null)
                    return NotFound($"Affiliate with ID {id} not found.");
                affiliate.IsActive = true; // Assuming IsActive is a property in AffiliateDTO
                var response = await _affiliateRepository.ActiveAffiliateAsync(id);
                if (!response.IsSuccess)
                    return BadRequest(response.ErrorMessage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("UpdatePassword")]
        public async Task<ActionResult<ResponseDTO>> UpdatePassword(string username, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
                return BadRequest("New password cannot be empty.");
            try
            {
                var affiliate = await _affiliateRepository.GetAffiliateDTOAsyncByuserName(username);
                if (affiliate == null)
                    return NotFound($"Affiliate with username not found.");
                affiliate.Password = newPassword; // Assuming Password is a property in AffiliateDTO
                var response = await _affiliateRepository.UpdatePasswordForAffiliateAsync(username, newPassword);
                if (!response.IsSuccess)
                    return BadRequest(response.ErrorMessage);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ValidateAffiliateCode")]
        public async Task<ActionResult<ResponseDTO>> ValidateAffiliateCode(string affiliateid)
        {
            if(string.IsNullOrEmpty(affiliateid))
                return BadRequest("Affiliate ID cannot be null or empty.");
            
            try
            {
                var response = await _affiliateRepository.ValidateAffiliateCode(affiliateid);
                if (!response.IsSuccess)
                    return BadRequest(response.ErrorMessage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
