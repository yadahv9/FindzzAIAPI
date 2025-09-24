using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.RecruiterRepositories;
using ProjectAgaman.Repositories.RolesRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecruiterController : ControllerBase
    {
        private readonly IRecruiterRepository _recruiterRepository;
        public RecruiterController(IRecruiterRepository recruiterRepository)
        {
            _recruiterRepository = recruiterRepository;
        }

        [HttpPost("GetJobseekrsByRecruiterId")]

        public async Task<IActionResult> Get(string recruiterId)
        {
            try
            {
                var jobseekrs =await _recruiterRepository.GetJobSeekersByRecruiterId(recruiterId);
                if (jobseekrs == null)
                {
                    return NotFound($"No job seekers assigned");
                }
                return Ok(jobseekrs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetJobCountsPerJobSeeker")]
        public async Task<ActionResult<List<JobSeekerJobCountDTO>>> GetJobCountsPerJobSeeker(int recruiterId)
        {
            try
            {
                var jobCounts = await _recruiterRepository.GetJobCountsPerJobSeeker(recruiterId);
                /*if (jobCounts == null || !jobCounts.Any())
                {
                    return NotFound($"No job counts found for recruiter with ID {recruiterId}.");
                }*/
                return Ok(jobCounts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

    }
}
