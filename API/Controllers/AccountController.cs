using API.DTOs.Account;
using API.Models;
using API.Services;
using Google.Apis.Auth;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using User = API.Models.User;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration configuration;
        private readonly EmailService emailService;
        private readonly UserManager<User> _userManager;
        private readonly HttpClient _facebookHttpClient;
        public AccountController(JWTService jwtService, 
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            IConfiguration configuration,
            EmailService emailService)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            this.configuration = configuration;
            this.emailService = emailService;
            _facebookHttpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://graph.facebook.com/v24.0/")
            };
        }

        #region Private Helper Method
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            if(user.EmailConfirmed== false)
            {
                return Unauthorized("Please confirm your email.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid email or password.");
            }
            return await CreateApplicationUserDto(user);
        }

        [HttpPost("login-with-third-party")]
        public async Task<ActionResult<UserDto>> LoginWithExternal(LoginWithExternal model)
        {
            if (model.Provider.Equals(SD.Facebook))
            {
                try
                {
                    if (!FacebookValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unabled to login with facebook");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unabled to login with facebook");
                }

            }
            else if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unabled to login with google");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unabled to login with google");
                }
            }
            else
            {
                return BadRequest("Invalid Provider");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(x=>x.UserName==model.UserId && x.Provider==model.Provider);
            if (user == null)
            {
                return Unauthorized("Unable to find your account");
            }
            return await CreateApplicationUserDto(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto model)
        {
            if(await CheckEmailExistsasync(model.Email))
            {
                return BadRequest($"Email is already in use account {model.Email}. Please try with another email address.");
            }

            var userToAdd = new User
            {
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                //EmailConfirmed=true,
            };

            var result = await _userManager.CreateAsync(userToAdd,model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            try
            {
                if(await SendConfirmEmailAsync(userToAdd))
                {
                    return Ok(new JsonResult(new { title = "Account Created", message = "Your account has been created, please confirm your email." }));
                }
                return BadRequest("Failed in send email confirmation");
            }
            catch(Exception ex)
            {
                return BadRequest("Failed in send email confirmation");
            }
        }


        [HttpPost("register-with-third-party")]
        public async Task<ActionResult<UserDto>> RegisterWithThirdParty(RegisterWithExternal model)
        {
            if (model.Provider.Equals(SD.Facebook))
            {
                try
                {
                    if (!FacebookValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unabled to register with facebook");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unabled to register with facebook");
                }
               
            }else if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unabled to register with google");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unabled to register with google");
                }
            }
            var user = await _userManager.FindByNameAsync(model.UserId);
            if(user != null)
            {
                return BadRequest(string.Format("You have already an account.Please login with your {0}", model.Provider));
            }
            var userToAdd = new User
            {
                UserName = model.UserId,
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                Provider =model.Provider
            };

            var result = await _userManager.CreateAsync(userToAdd);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return await CreateApplicationUserDto(userToAdd);
        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("This email address has not been registered yet");

            if (user.EmailConfirmed == true) return BadRequest("Your email was confirm before. Please login to your account");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Email Confirm", message = "Your email address is confirm. You can login now" }));
                }
                return BadRequest("Invalid Token. Please try again");

            }
            catch (Exception ex) {
                return BadRequest("Invalid Token. Please try again");
            }

        }

        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Invalid Email");
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized("This email address has not been register yet");
            if (user.EmailConfirmed == true) return BadRequest("Your email address was confirm before. Please login to your account");

            try
            {
                if(await SendConfirmEmailAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Confirm link send", message = "Please confirm your email address." }));
                }
                return BadRequest("Failed to send Email.");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send Email.");
            }

        }

        [HttpPost("forget-username-or-password/{email}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Invalid email");
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized("This email address has not been register yet");
            if (user.EmailConfirmed == false) return BadRequest("Please confirm ypour email address first.");

            try
            {
                if (await SendForgotUsernameOrPassword(user))
                {
                    return Ok(new JsonResult(new { title = "Forgot username or password", message = "Please check your email." }));
                }
                return BadRequest("Failed to send Email. Please contract with admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send Email.");
            }

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("This email address has not been register yet");
            if (user.EmailConfirmed == false) return BadRequest("Please confirm ypour email address first.");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken,model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Reset password success", message = "Your password has been reset." }));
                }
                return BadRequest("Invalid token. Please try again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid token. Please try again");
            }

        }


        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return await CreateApplicationUserDto(user);
        }

        private async Task<UserDto> CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = await  _jwtService.CreateJWT(user)
            };
        }

        private async Task<bool> CheckEmailExistsasync(string email)
        {
            return await _userManager.Users.AnyAsync(x=>x.Email==email.ToLower());
        }

        private async Task<bool> SendForgotUsernameOrPassword(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";


            var body = $"<p>Hello : {user.FirstName} {user.LastName}</p>" +
               $"<p>Username : {user.UserName}</p>"+
               $"<p>In order to reset your password by clicking on the following link.</p>" +
               $"<p><a href=\"{url}\">Click here</a></p>" +
               $"<p>Thank you,</p>" +
               $"<br>{configuration["Email:ApplicationName"]}";
            var emailSend = new EmailSendDto(user.Email, body, "Forget username & password");
            return await emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> SendConfirmEmailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";
            var body = $"<p>Hello : {user.FirstName} {user.LastName}</p>" +
                $"<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                $"<p>Thank you,</p>" +
                $"<br>{configuration["Email:ApplicationName"]}";
            var emailSend = new EmailSendDto(user.Email, body,"Confirm your email");
            return await emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> FacebookValidatedAsync(string accessToken,string userId)
        {
            var appId = configuration["Facebook:AppId"];
            var appSecret = configuration["Facebook:AppSecrect"];

            var appAccessToken = $"{appId}|{appSecret}";

            var fbResult = await _facebookHttpClient.GetFromJsonAsync<facebookResultDto>($"debug_token?input_token={accessToken}&access_token={appAccessToken}");
            if(fbResult == null || fbResult.Data.Is_Valid == false || !fbResult.Data.user_Id.Equals(userId))
            {
                return false;
            }
            return true;
        }
        private async Task<bool> GoogleValidatedAsync(string accessToken, string userId)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken);
            if (!payload.Audience.Equals(configuration["Google:ClientId"]))
            {
                return false;
            }
            if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
            {
                return false;
            }
            if(payload.ExpirationTimeSeconds == null)
            {
                return false;
            }

            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime expiaration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
            if (now > expiaration) {
                return false;
            }

            if (!payload.Subject.Equals(userId))
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
