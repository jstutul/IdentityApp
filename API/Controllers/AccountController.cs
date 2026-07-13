using API.DTOs.Account;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

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
                return Unauthorized("Email is not confirmed.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid email or password.");
            }
            return CreateApplicationUserDto(user);
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
            return CreateApplicationUserDto(user);
        }

        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJWT(user)
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
            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ResetPasswordPath"]}?token={user.Email}";


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
            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ConfirmEmailPath"]}?token={user.Email}";
            var body = $"<p>Hello : {user.FirstName} {user.LastName}</p>" +
                $"<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                $"<p>Thank you,</p>" +
                $"<br>{configuration["Email:ApplicationName"]}";
            var emailSend = new EmailSendDto(user.Email, body,"Confirm your email");
            return await emailService.SendEmailAsync(emailSend);
        }

        #endregion
    }
}
