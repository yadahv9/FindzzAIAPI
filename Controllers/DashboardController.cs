using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.DashboardRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

      /*  [Authorize(Roles = "Admin")]*/
        [HttpGet("GetDashboardCounts")]
        public async Task<ActionResult<DashboardCountsDTO>> GetDashboardCounts()
        {
            try
            {
                var dashboardCounts = await _dashboardRepository.GetDashboardCounts();
                if (dashboardCounts == null)
                {
                    return NotFound("Dashboard counts not found.");
                }
                return Ok(dashboardCounts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetInterviewQuestionAndAnswer")]
        public async Task<ActionResult<InterviewQuestionAndAnswerResponseDTO>> GetInterviewQuestionAndAnswer(int jobId)
        {
            try
            {
                var response = await _dashboardRepository.GetInterviewQuestionAndAnswer(jobId);
                if (response == null)
                {
                    return NotFound("Interview questions and answers not found for the specified job ID.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetDashboardCountsForAffiliate")]
        public async Task<ActionResult<DashboardCountsDTO>> GetDashboardCountsForAffiliate(int affiliateId)
        {
            try
            {
                if (affiliateId <= 0)
                {
                    return BadRequest("Invalid affiliate ID.");
                }
                var dashboardCounts = await _dashboardRepository.GetDashboardCountsForAffiliate(affiliateId);
                if (dashboardCounts == null)
                {
                    return NotFound("Dashboard counts for affiliate not found.");
                }
                return Ok(dashboardCounts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
