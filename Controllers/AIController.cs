using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.AzureOpenAIRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IAzureOpenAIRepository _azureOpenAIRepository;

        public AIController(IAzureOpenAIRepository azureOpenAIRepository)
        {
            _azureOpenAIRepository = azureOpenAIRepository;
        }

        [HttpPost("GenerateTextAsync")]
        public async Task<ActionResult<OpenAIResponseDTO>> GenerateTextAsync(string prompt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    return BadRequest(new OpenAIResponseDTO { ErrorMessage = "Prompt cannot be empty." });
                }
                var response = await _azureOpenAIRepository.GenerateClaudeAIResponse(prompt, "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",null);

                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessage);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");

            }
            
        }
    }
}
