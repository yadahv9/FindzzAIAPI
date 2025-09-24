using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Core.Helpers;
using ProjectAgaman.Repositories.AffiliateRepositories;
using ProjectAgaman.Repositories.EmailSender;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffiliateAuthController : ControllerBase
    {
        private readonly IAffiliateRepository _affiliateRepository;
        private readonly JwtTokenService _jwt;
        private readonly IConfiguration _configuration;
        private readonly DbContext _dbContext;
        private readonly IEmailSender _emailSender;
        
        public AffiliateAuthController(IAffiliateRepository affiliateRepository, JwtTokenService jwt, IConfiguration configuration, DbContext dbContext, IEmailSender emailSender)
        {
            _affiliateRepository = affiliateRepository;
            _jwt = jwt;
            _configuration = configuration;
            _dbContext = dbContext;
            _emailSender = emailSender;
          
        }

        [HttpPost("AffiliateLogin")]
        public async Task<IActionResult> AffiliateLogin([FromBody] AffiliateLoginDTO login)
        {
            try
            {
                if (login == null)
                    return BadRequest("Invalid client request");
                if (string.IsNullOrEmpty(login.UsernameOrEmail) || string.IsNullOrEmpty(login.Password))
                    return BadRequest("Username/Email and password are required");
                if (string.IsNullOrEmpty(login.CaptchaToken))
                    return BadRequest("Captcha is required");

                // Validate captcha
                var captchaValid = await ValidateCaptchaAsync(login.CaptchaToken);
                if (!captchaValid)
                    return Unauthorized("Captcha validation failed");

                // Try to retrieve affiliate by email first, then by username
                AffiliateDTO affiliate = null;
                
                // Check if input looks like an email (contains @)
                if (login.UsernameOrEmail.Contains("@"))
                {
                    affiliate = await _affiliateRepository.GetAffiliateDTOAsyncByEmailId(login.UsernameOrEmail);
                }
                else
                {
                    // Try to get affiliate by username first
                    affiliate = await _affiliateRepository.GetAffiliateDTOAsyncByuserName(login.UsernameOrEmail);
                }

                // If affiliate not found and input doesn't contain @, try email lookup as fallback
                if (affiliate == null && !login.UsernameOrEmail.Contains("@"))
                {
                    affiliate = await _affiliateRepository.GetAffiliateDTOAsyncByEmailId(login.UsernameOrEmail);
                }

                if (affiliate == null)
                {
                    return Unauthorized("User is not FOUND");
                }
                if (!affiliate.IsActive)
                {
                    return Unauthorized("User is not active");
                }
                // Hash the incoming password using SHA256  
                var hashedPassword = EncryptionHelper.DecryptString(_configuration["EncryptionKey"], affiliate.Password);
                if (hashedPassword != login.Password)
                    return Unauthorized("Invalid credentials");

                var token = await _jwt.GenerateTokenForAffiliate(affiliate, "Affiliate");

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
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

                var secretKey =settings.settingvalue;
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


        [HttpPost("ForgotPasswordForAffiliate")]
        public async Task<ActionResult<ResponseDTO>> ForgotPassword([FromBody] ForgotPasswordForAffiliateDTO model)
        {
            try
            {
                var affiliate = await _affiliateRepository.GetAffiliateDTOAsyncByEmailId(model.Emailid);
                if (affiliate == null)
                {
                    return NotFound(new ResponseDTO { ErrorMessage = "Email not found. Please register first." });
                }
                if (!affiliate.IsActive)
                    return BadRequest(new ResponseDTO { ErrorMessage = "User is not active" });

                var otp = new Random().Next(100000, 1000000).ToString();


                var data = await _affiliateRepository.UpdateOtpForAffiliate(model.Emailid, otp);

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
                var result = await _emailSender.SendForgotPasswordOtp(affiliate.Firstname, affiliate.EmailAddress, otp, settings.settingvalue);
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

        [HttpPost("VerifyOTPForAffiliate")]
        public async Task<ActionResult<ResponseDTO>> VerifyOtp([FromBody] VerifyOtpForAffiliateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Emailid) || string.IsNullOrWhiteSpace(model.Otp))
            {
                return BadRequest(new ResponseDTO { ErrorMessage = "Email and OTP are required." });
            }

            var isValidOtp = await _affiliateRepository.GetOtpForAffiliate(model.Emailid);

            if (string.IsNullOrEmpty(isValidOtp) || isValidOtp != model.Otp)
            {
                return Unauthorized(new ResponseDTO { ErrorMessage = "Invalid OTP." });
            }

            return Ok(new ResponseDTO());
        }

        [HttpPost("ResetPasswordForAffiliate")]
        public async Task<ActionResult<ResponseDTO>> ResetPassword([FromBody] ResetPasswordForAffiliateDTO model)
        {
            try
            {
                // 1. Validate input
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    return BadRequest(new ResponseDTO { ErrorMessage = "Email and password are required." });
                }

                // 2. Find user by email
                var user = await _affiliateRepository.GetAffiliateDTOAsyncByEmailId(model.Email);
                if (user == null)
                {
                    return NotFound(new ResponseDTO { ErrorMessage = "User not found." });
                }
                if (!user.IsActive)
                {
                    return BadRequest(new ResponseDTO { ErrorMessage = "User is not active" });

                }

                var response = await _affiliateRepository.UpdatePasswordForAffiliateAsync(model.Email, model.NewPassword);

                if (response == null)
                {
                    return BadRequest(new ResponseDTO { ErrorMessage = "Error Occuried While Updating Password" });
                }
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



    }
}
