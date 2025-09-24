using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Core.Helpers;
using ProjectAgaman.Repositories.AffiliateRepositories;
using ProjectAgaman.Repositories.EmailSender;
using ProjectAgaman.Repositories.RolesRepositories;
using ProjectAgaman.Repositories.UsersRepositories;
using System.Net.NetworkInformation;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwt;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _accessor;
        private readonly DbContext _dbContext;

        public AuthController(JwtTokenService jwt, IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration configuration, IEmailSender emailSender, IHttpContextAccessor accessor, DbContext dbContext)
        {
            _jwt = jwt;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
            _emailSender = emailSender;
            _accessor = accessor;
            _dbContext = dbContext;

        }

        [HttpGet("GetAllRecruiter")]
        public async Task<ActionResult<List<RecruiterDto>>> GetAllUsers()
        {
            try
            {
                var recruiters = await _userRepository.GetAllRecruiters();
                return Ok(recruiters);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                var captchaValid = await ValidateCaptchaAsync(login.CaptchaToken);
                if (!captchaValid)
                    return Unauthorized("Captcha validation failed");
                if (login == null)
                    return BadRequest("Invalid client request");

                if (string.IsNullOrEmpty(login.UsernameOrEmail) || string.IsNullOrEmpty(login.Password))
                    return BadRequest("Username/Email and password are required");

                // Try to retrieve user by email first, then by username
                UsersDTO user = null;
                
                // Check if input looks like an email (contains @)
                if (login.UsernameOrEmail.Contains("@"))
                {
                    user = await _userRepository.GetUserByEmail(login.UsernameOrEmail);
                }
                else
                {
                    // Try to get user by username first
                    user = await _userRepository.GetUserByUsername(login.UsernameOrEmail);
                }

                // If user not found and input doesn't contain @, try email lookup as fallback
                if (user == null && !login.UsernameOrEmail.Contains("@"))
                {
                    user = await _userRepository.GetUserByEmail(login.UsernameOrEmail);
                }

                if (user == null)
                {
                    return Unauthorized("User is not FOUND");
                }

                if (!user.IsActive)
                {
                    return Unauthorized("User is not active");
                }
                // var ipsfromsetting = await _userRepository.GetAllIpFromSett();

                // Hash the incoming password using SHA256  
                var hashedPassword = EncryptionHelper.DecryptString(_configuration["EncryptionKey"], user.Password);

                if (hashedPassword != login.Password)
                    return Unauthorized("Invalid credentials");

                if (user.IsActive == false)
                    return Unauthorized("User is not active");

                var roles = await _roleRepository.GetRoleById(user.FkRoleId);

                var token = await _jwt.GenerateToken(user, roles.rolename);
               /* if (roles.pkroleId == 3)
                {
                    var data = await GetLocalIPv4Async();
                    if (data != user.IpAddress)
                    {
                        return Unauthorized("IP address mismatch");
                    }

                }*/
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<ResponseDTO>> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            try
            {
                var user = await _userRepository.GetUserByEmail(model.EmailId);
                if (user == null)
                {
                    return NotFound(new ResponseDTO { ErrorMessage = "Email not found. Please register first." });
                }

                var otp = new Random().Next(100000, 1000000).ToString();


                var data = await _userRepository.UpdateOtpForUser(model.EmailId, otp);

                if (!data.IsSuccess)
                {
                    return BadRequest(new ResponseDTO { ErrorMessage = "Failed to update OTP." });

                }

                using var connection = _dbContext.CreateConnection();
                var dapperHelper = new DapperHelper(connection);

                var parameters = new DynamicParameters();
                parameters.Add("p_settingname", "ForgotPasswordEmailTemplate");
                var settings = await dapperHelper.ExecuteStoredProcedureSingleAsync<SettingDTO>("spgetemailtemplate", parameters);

                if (settings == null || string.IsNullOrEmpty(settings.settingvalue))
                {
                    return new ResponseDTO
                    {
                        ErrorMessage = "Email template not found."
                    };
                }
                var result = await _emailSender.SendForgotPasswordOtp(user.Firstname, model.EmailId, otp, settings.settingvalue);
                if (!string.IsNullOrEmpty(result?.ErrorMessage))
                {
                    return BadRequest(new ResponseDTO { ErrorMessage = "Failed to send OTP." });
                }

                return Ok(new ResponseDTO());

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { ErrorMessage = ex.Message });
            }
        }

        [HttpPost("VerifyOTP")]
        public async Task<ActionResult<ResponseDTO>> VerifyOtp([FromBody] VerifyOtpDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Otp))
            {
                return BadRequest(new ResponseDTO { ErrorMessage = "Email and OTP are required." });
            }

            var isValidOtp = await _userRepository.GetOtpForUser(model.Email);

            if (string.IsNullOrEmpty(isValidOtp) || isValidOtp != model.Otp)
            {
                return Unauthorized(new ResponseDTO { ErrorMessage = "Invalid OTP." });
            }

            return Ok(new ResponseDTO());
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult<ResponseDTO>> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            try
            {
                // 1. Validate input
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    return BadRequest(new ResponseDTO { ErrorMessage = "Email and password are required." });
                }

                // 2. Find user by email
                var user = await _userRepository.GetUserByEmail(model.Email);
                if (user == null)
                {
                    return NotFound(new ResponseDTO { ErrorMessage = "User not found." });
                }

                var response = await _userRepository.UpdateNewPasswordForUser(model.Email, model.NewPassword);

                // 5. Return success
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the error if necessary
                return StatusCode(500, new ResponseDTO
                {
                    ErrorMessage = "An error occurred while resetting the password."
                });
            }
        }




        /* private async Task<string> GetLocalIPv4Async(NetworkInterfaceType _type)
         {
             *//*string output = "";
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

             return await Task.FromResult(output);*//*

             string output = "";
             if (Request.Headers.ContainsKey("X-Forwarded-For"))
             {
                 // output = Request.Headers.FirstOrDefault(x => x.Key == "X-Forwarded-For").Value.ToString().Split(":")[0];
                 output = Request.Headers.FirstOrDefault(x => x.Key == "X-Forwarded-For").Value.ToString().Trim();
                 Console.WriteLine(Request.Headers.FirstOrDefault(x => x.Key == "X-Forwarded-For").Value.ToString().Trim());
             }
             else
                 output = _accessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
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
