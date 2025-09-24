using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Core.Helpers;
using ProjectAgaman.Core.Models;
using ProjectAgaman.Repositories.RolesRepositories;
using ProjectAgaman.Repositories.UsersRepositories;
using System.Configuration;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbContext _dbContext;
        public readonly IUserRepository _userRepository;
        public readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;
        public UsersController(IUserRepository userRepository, IConfiguration configuration, IRoleRepository roleRepository, DbContext dbContext)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UsersDTO>>> GetAllUsers(int? affiliateid)
        {
            try
            {
                var users = await _userRepository.GetAllUsers(affiliateid);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

       
        [HttpGet("GetPaginatedUsersAsync")]
        public async Task<ActionResult<PaginationEntityDto<UsersDTO>>> GetPaginatedUsersAsync(int pageIndex, int pageSize, string? searchname)
        {
            try
            {
                var paginatedUsers = await _userRepository.GetPaginatedUsersAsync(pageIndex, pageSize, searchname);
                return Ok(paginatedUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("AddUser")]
        public async Task<ActionResult<ResponseDTO>> AddUser([FromForm] UsersDTO userDTO, [FromForm] string? CaptchaToken)
        {
            if (userDTO == null)
            {
                return BadRequest("User data is null.");
            }
            try
            {
                if(CaptchaToken != null && CaptchaToken != "")
                {
                    var captchaValid = await ValidateCaptchaAsync(CaptchaToken);
                    if (!captchaValid)
                        return Unauthorized("Captcha validation failed");
                }
               
                if (userDTO.IpAddress == null || userDTO.IpAddress == "")
                {
                    userDTO.IpAddress = await GetLocalIPv4Async();
                }
                var response = await _userRepository.AddUser(userDTO);
                /*if (!response.IsSuccess)
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
      
        [HttpPut("UpdateUser")]
        public async Task<ActionResult<ResponseDTO>> UpdateUser([FromForm] UsersDTO userDTO)
        {
           
            if (userDTO == null)
            {
                return BadRequest("User data is null.");
            }
            try
            {
               /* var existingJob = await _userRepository.GetUserById(userDTO.PkUserId);
                if (existingJob == null)
                {
                    return NotFound($"No job found.");
                }
*/
                if (userDTO.IpAddress == null || userDTO.IpAddress == "")
                {
                    userDTO.IpAddress = await GetLocalIPv4Async();
                }

                var response = await _userRepository.UpdateUser(userDTO);
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
        
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<ResponseDTO>> DeleteUser(DeleteOrActiveDTO deleteOrActiveUserDTO)
        {
            try
            {
                if (deleteOrActiveUserDTO == null || deleteOrActiveUserDTO.id <= 0)
                {
                    return BadRequest("Invalid user ID.");
                }
                var response = await _userRepository.DeleteUser(deleteOrActiveUserDTO);
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
       
        [HttpPost("DeleteListOfUsers")]
        public async Task<ActionResult<ResponseDTO>> DeleteListOfUsers([FromBody] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return BadRequest("User IDs list is null or empty.");
            }
            try
            {
                var response = await _userRepository.DeleteListOfUsers(userIds);
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

        [HttpPost("GetUsersByAffiliateId")]
        public async Task<ActionResult<IEnumerable<UsersDTO>>> GetUsersByAffiliateId(int affiliateId)
        {
            try
            {
                if (affiliateId <= 0)
                {
                    return BadRequest("Invalid affiliate ID.");
                }
                var users = await _userRepository.GetUsersByAffiliateId(affiliateId);
                /*if (users == null || !users.Any())
                {
                    return NotFound("No users found for the specified affiliate ID.");
                }*/
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("GetuserDetailsByRoleName")]
        public async Task<ActionResult<IEnumerable<UsersDTO>>> GetRecruitersDetailsbyRoleId(string? roleName)
        {
            try
            {
                var roles = await _roleRepository.GetRolebyName(roleName);
                var recruiters = await _userRepository.GetRecruitersDetailsbyRoleId(roles.pkroleId);
                if (recruiters == null || !recruiters.Any())
                {
                    return NotFound("No recruiters found for the specified role ID.");
                }
                return Ok(recruiters);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("UploadResume")]
        public async Task<ActionResult<string>> UploadResumeAsync(IFormFile file, int pkuserId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is null or empty.");
            }
            try
            {
                var response = await _userRepository.UploadResumeAsync(file, pkuserId);
                if (response == null)
                {
                    return BadRequest();
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }

        [Authorize]
        [HttpPost("GetUserById")]
        public async Task<ActionResult> GetUserById(int userId)
        {
            if (userId == 0)
            {
                return BadRequest("User not found");
            }
            try
            {
                var response = await _userRepository.GetUserById(userId);
                response.Password =EncryptionHelper.DecryptString(_configuration["EncryptionKey"], response.Password);
                if (response == null)
                {
                    return NotFound("No user found for the specified  ID.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }

        [Authorize]
        [HttpPost("UpdatePackageByUserId")]
        public async Task<ActionResult> UpdatePackageByUserId(int userId,int packageId)
        {
            if (userId == 0)
            {
                return BadRequest("User not found");
            }
            try
            {
                var response = await _userRepository.UpdatePackageByUserId(userId, packageId);
                if (response == null)
                {
                    return NotFound("No user found for the specified  ID.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost("UpdateResumeForUser")]
        public async Task<ActionResult<UpdateResumeForUserResponseDTO>> UpdateResumeForUser([FromForm] UpdateResumeforUserDTO updateResumeforUserDTO)
        {
            try
            {
                if (updateResumeforUserDTO == null || updateResumeforUserDTO.UserId <= 0 || updateResumeforUserDTO.ResumeFile ==null)
                {
                    return BadRequest("Invalid user ID.");
                }
                var response = await _userRepository.UpdateResumeForUser(updateResumeforUserDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while updating resume.");
            }
        }

        [HttpPost("UpdateFreeIQUsedByUserId")]
        public async Task<ActionResult<ResponseDTO>> UpdateFreeIQUsedByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid user ID.");
                }
                var response = await _userRepository.UpdateFreeIQUsedByUserId(userId);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
                  
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while updating Free IQ usage.");
            }
        }
        /* private async Task<string> GetLocalIPv4Async(NetworkInterfaceType _type)
         {
             string output = "";
             foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
             {
                 if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                 {
                     foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                     {
                         if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                         {
                             output = ip.Address.ToString();
                         }
                     }
                 }
             }
             return await Task.FromResult(output);
         }*/

        private async Task<string> GetLocalIPv4Async()
        {
            var context = this.HttpContext;

            // 1) If behind a proxy like Nginx, it will set X-Forwarded-For
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var header))
            {
                // May contain a comma-separated list: client, proxy1, proxy2…
                return header.ToString()
                             .Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(x => x.Trim())
                             .FirstOrDefault()!;
            }

            // 2) Otherwise fall back to the direct remote address
            return context.Connection.RemoteIpAddress
                          ?.MapToIPv4()
                          .ToString()
                   ?? string.Empty;
        }

        private async Task<bool> ValidateCaptchaAsync(string captchaToken)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();
                var dapperHelper = new DapperHelper(connection);
                var parameters = new DynamicParameters();
                parameters.Add("p_settingname", "recaptchasecretkey");
                var settings = await dapperHelper.ExecuteStoredProcedureSingleAsync<SettingDTO>("spgetemailtemplate", parameters);
                if (settings == null || string.IsNullOrEmpty(settings.settingvalue))
                {
                    return false; // Secret key not found
                }

                var secretKey = settings.settingvalue;
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaToken}",
                    null
                );
                var json = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json);
                if (result.success != true)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
