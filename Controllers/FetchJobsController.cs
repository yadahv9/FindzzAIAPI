using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.FetchJobsRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FetchJobsController : ControllerBase
    {
        private readonly IFetchJobsRepository _fetchJobsRepository;

        public FetchJobsController(IFetchJobsRepository fetchJobsRepository)
        {
            _fetchJobsRepository = fetchJobsRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("AdzunaJobFetcher")]
        public async Task<ActionResult<FetchJobsResponseDTO>> AdzunaJobFetcher([FromBody] FetchJobsDTO fetchJobsDTO)
        {
            if (fetchJobsDTO == null)
            {
                return BadRequest("Fetch jobs data is null.");
            }
            try
            {
                var response = await _fetchJobsRepository.AdzunaJobFetcher(fetchJobsDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("RapidJobFetcher")]
        public async Task<ActionResult<FetchJobsResponseDTO>> RapidJobFetcher([FromBody] FetchJobsDTO fetchJobsDTO)
        {
            if (fetchJobsDTO == null)
            {
                return BadRequest("Fetch jobs data is null.");
            }
            try
            {
                var response = await _fetchJobsRepository.RapidJobFetcher(fetchJobsDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("GoogleJobFetcher")]
        public async Task<ActionResult<FetchJobsResponseDTO>> GoogleJobFetcher([FromBody] FetchJobsDTO fetchJobsDTO)
        {
            if (fetchJobsDTO == null)
            {
                return BadRequest("Fetch jobs data is null.");
            }
            try
            {
                var response = await _fetchJobsRepository.GoogleJobFetcher(fetchJobsDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("FantasticsJobsFetcher")]
        public async Task<ActionResult<FetchJobsResponseDTO>> FantasticsJobsFetcher([FromBody] FetchJobsDTO fetchJobsDTO)
        {
            if (fetchJobsDTO == null)
            {
                return BadRequest("Fetch jobs data is null.");
            }
            try
            {
                var response = await _fetchJobsRepository.FantasticsJobsFetcher(fetchJobsDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("IndeedScraperJobFetcher")]
        public async Task<ActionResult<FetchJobsResponseDTO>> IndeedScraperJobFetcher([FromBody] FetchJobsDTO fetchJobsDTO)
        {
            if (fetchJobsDTO == null)
            {
                return BadRequest("Fetch jobs data is null.");
            }
            try
            {
                var response = await _fetchJobsRepository.IndeedScraperJobsFetcher(fetchJobsDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CareerJetJobFetcher")]
        public async Task<ActionResult<List<JobCsvRowDTO>>> CareerJetJobFetcher(CareerJetFetchJobsDTO fetchJobsDTO)
        {
            if (fetchJobsDTO == null)
            {
                return BadRequest("Fetch jobs data is null.");
            }
            try
            {
                var jobRows = await _fetchJobsRepository.CareerJetJobFetcher(fetchJobsDTO);
                if (jobRows == null || !jobRows.Any())
                {
                    return NotFound("No jobs found for the given criteria.");
                }
                return Ok(jobRows);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }

        [HttpGet("GetAllFetchJobs")]
        public async Task<ActionResult<IEnumerable<JobInfoDTO>>> GetAllFetchJobs()
        {
            try
            {
                var fetchJobs = await _fetchJobsRepository.GetAllJobsAsync();
                return Ok(fetchJobs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetJobInfoById")]
        public async Task<ActionResult<JobInfoDTO>> GetJobInfoById(int jobId)
        {
            if (jobId <= 0)
            {
                return BadRequest("Invalid job ID.");
            }
            try
            {
                var jobInfo = await _fetchJobsRepository.GetJobInfoByIdAsync(jobId);
                if (jobInfo == null)
                {
                    return NotFound($"Job with ID {jobId} not found.");
                }
                return Ok(jobInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetJobInfoByTitleAndLocation")]
        public async Task<ActionResult<IEnumerable<JobInfoDTO>>> GetJobInfoByTitleAndLocation([FromBody] JobInfoByTitleAndLocationDTO jobInfoByTitleAndLocationDTO)
        {
            if (jobInfoByTitleAndLocationDTO == null)
            {
                return BadRequest("Job info data is null.");
            }
            try
            {
                var jobInfos = await _fetchJobsRepository.GetJobInfoByTitleAndLocation(jobInfoByTitleAndLocationDTO);
                return Ok(jobInfos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ModifyResumeBasedonJobDescription")]
        public async Task<ActionResult<UploadResumeResponseDTO>> ModifyResumeBasedonJobDescription(int jobId, int userId)
        {

            if (jobId <= 0 || userId <= 0)
            {
                return BadRequest("Invalid job ID or user ID.");
            }
            try
            {
                var response = await _fetchJobsRepository.ModifyResumeBasedonJobDescription(jobId, userId);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("UpdateJobDescriptions")]
        public async Task<ActionResult<FetchJobsResponseDTO>> UpdateJobDescriptions([FromBody] UpdateJobDescriptionDTO updateJobDescriptionDTO)
        {
            if (updateJobDescriptionDTO == null)
            {
                return BadRequest("Update job description data is null.");
            }
            try
            {
                var response = await _fetchJobsRepository.UpdateJobDescriptions(updateJobDescriptionDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
