using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.UserJobsRepositories;
using ProjectAgaman.Repositories.UsersRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserJobsController : ControllerBase
    {
        private readonly IUserJobsRepository _userJobsRepository;
        public UserJobsController(IUserJobsRepository userJobsRepository)
        {
            _userJobsRepository = userJobsRepository;
        }

        [HttpGet("GetAllUserJobss")]
        public async Task<ActionResult<IEnumerable<UserJobsDTO>>> Get()
        {
            try
            {
                var userJobs = await _userJobsRepository.GetAllUserJobsAsync();

                return Ok(userJobs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetPaginatedUserJobsAsync")]
        public async Task<ActionResult<PaginationEntityDto<UserJobsDTO>>> GetPaginatedUserJobsAsync(int pageIndex, int pageSize, string? searchname)
        {
            try
            {
                var paginatedUserJobs = await _userJobsRepository.GetPaginatedUserJobsAsync(pageIndex, pageSize, searchname);
                if (paginatedUserJobs == null)
                {
                    return NotFound("No user jobs found.");
                }
                return Ok(paginatedUserJobs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetUserJobById/{id}")]
        public async Task<IActionResult> GetUserJobById(int id)
        {
            try
            {
                var userJob = _userJobsRepository.GetUserJobByIdAsync(id).Result;
                if (userJob == null)
                {
                    return NotFound($"No job found with ID {id}.");
                }
                return Ok(userJob);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetUserJobsCounts")]
        public async Task<ActionResult<UserJobsCountDTO>> GetUserJobsCounts(int id)
        {
            try
            {
                var userJobsCount = await _userJobsRepository.GetUserJobsCounts(id);
                if (userJobsCount == null)
                {
                    return NotFound($"No job counts found for user with ID {id}.");
                }
                return Ok(userJobsCount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetUserJobsCountsBasedOnDates")]
        public async Task<ActionResult<UserJobsCountBasedOnDates>> GetUserJobsCountsBasedOnDates([FromBody] UserJobsCountRequsetDTO userJobsCountDto)
        {
            try
            {
                if (userJobsCountDto == null)
                {
                    return BadRequest("User jobs count data is null.");
                }
                var userJobsCount = await _userJobsRepository.GetUserJobsCounts(userJobsCountDto);
                if (userJobsCount == null)
                {
                    return NotFound("No job counts found for the specified dates.");
                }
                return Ok(userJobsCount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddUserJob")]
        public async Task<ActionResult<ResponseDTO>> AddUserJob([FromBody] UserJobsDTO userJobDto)
        {
            try
            {
                if (userJobDto == null)
                {
                    return BadRequest("User job data is null.");
                }
                var response = await _userJobsRepository.AddUserJobAsync(userJobDto);
               /* if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }*/
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");

            }


        }

        [HttpPut("UpdateUserJob")]
        public async Task<ActionResult<ResponseDTO>> UpdateUserJob([FromBody] UserJobsDTO userJobDto)
        {
            try
            {
                if (userJobDto == null)
                {
                    return BadRequest("User job data is null.");
                }
                var existingUser = await _userJobsRepository.GetUserJobByIdAsync(userJobDto.PKUserJobId);
                if (existingUser == null)
                {
                    return new ResponseDTO
                    {
                        ErrorMessage = "Job already exists for the user with the same details."
                    };
                }



                var response = await _userJobsRepository.UpdateUserJobAsync(userJobDto);
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


        [HttpDelete("DeleteUserJob")]
        public async Task<ActionResult<ResponseDTO>> DeleteUserJobAsync(DeleteOrActiveDTO deleteOrActiveDTO)
        {
            try
            {
                if (deleteOrActiveDTO == null || deleteOrActiveDTO.id <= 0)
                {
                    return BadRequest("Invalid user job ID provided for deletion.");
                }
                var response = await _userJobsRepository.DeleteUserJobAsync(deleteOrActiveDTO);
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
        [HttpPost("DeleteListOfUserJob")]
        public async Task<ActionResult<ResponseDTO>> DeleteListOfUserJob([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return BadRequest("No IDs provided for deletion.");
                }
                var response = await _userJobsRepository.DeleteListOfUserJobAsync(ids);
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

        [HttpPost("GetUserJobsByDetails")]
        public async Task<ActionResult<bool>> GetUserJobsByDetails(int fkuserId, int jobId)
        {
            try
            {
                if( fkuserId <= 0 || jobId <= 0)
                {
                    return Ok(false);
                }

                var userJob = await _userJobsRepository.GetUserJobsByDetails(fkuserId, jobId);
                if (userJob == null)
                {
                    return Ok(false);
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateUserJobsProblem")]
        public async Task<ActionResult<ResponseDTO>> UpdateUserJobsProblem(UserJobsProblemDTO userJobsProblemDTO)
        {
            try
            {
                if (userJobsProblemDTO == null)
                {
                    return BadRequest("User jobs problem data is null.");
                }
                var response = await _userJobsRepository.UpdateUserJobsProblem(userJobsProblemDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }

        }

        [HttpGet("GetPaginatedProblemJobsAsync")]
        public async Task<ActionResult<PaginationEntityDto<ProblemJobsDTO>>> GetPaginatedProblemJobsAsync(int pageIndex, int pageSize, string? searchname)
        {
            try
            {
                var paginatedProblemJobs = await _userJobsRepository.GetPaginatedProblemJobsAsync(pageIndex, pageSize, searchname);
                if (paginatedProblemJobs == null)
                {
                    return NotFound("No problem jobs found.");
                }
                return Ok(paginatedProblemJobs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("GetTotalUserJobsCountByAffiliateidAsync")]
        public async Task<ActionResult<IEnumerable<UserJobsDTO>>> GetTotalUserJobsCountByAffiliateidAsync(int userid)
        {
            try
            {
                if (userid == 0)
                {
                    return BadRequest("Invalid user ID.");
                }
                var userJobsDTOs = await _userJobsRepository.GetTotalUserJobsCountByAffiliateidAsync(userid);
                return Ok(userJobsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
