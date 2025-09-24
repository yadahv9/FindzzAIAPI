using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.SettingsRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsRepository _settingsRepository;
        public SettingsController(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        [HttpGet("GetAllSettings")]

        public async Task<ActionResult<IEnumerable<SettingDTO>>> GetAllSettings()
        {
            try
            {
                var settings = await _settingsRepository.GetAllSettings();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("AddSettings")]
        public async Task<ActionResult<ResponseDTO>> AddSettings([FromBody] SettingDTO settingDTO)
        {
            if (settingDTO == null)
            {
                return BadRequest("Setting data is null.");
            }
            var result = await _settingsRepository.AddSettings(settingDTO);
            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result);
        }
        [HttpPut("UpdateSettings")]
        public async Task<ActionResult<ResponseDTO>> UpdateSettings([FromBody] SettingDTO settingDTO)
        {
            if (settingDTO == null)
            {
                return BadRequest("Setting data is null.");
            }
            var result = await _settingsRepository.UpdateSettings(settingDTO);
            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result);
        }
    }
}
