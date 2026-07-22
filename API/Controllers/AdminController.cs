using API.DTOs.Admin;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize(Roles ="Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [HttpGet("get-members")]
        public async Task<ActionResult<IEnumerable<MemberViewDto>>> GetMember()
        {
            var members = await _userManager.Users.Where(x => x.UserName != SD.AdminUserName).Select(member => new MemberViewDto
            {
                Id=member.Id,
                UserName=member.UserName,
                FirstName=member.FirstName,
                LastName=member.LastName,
                DateCreated=member.DateCreated,
                IsLocked=_userManager.IsLockedOutAsync(member).GetAwaiter().GetResult(),
                Roles =_userManager.GetRolesAsync(member).GetAwaiter().GetResult()
            }).ToListAsync();
            return Ok(members);
        }

        [HttpGet("get-member/{id}")]
        public async Task<ActionResult<IEnumerable<MemberAddEditDto>>> GetMember(string id)
        {
            var members = await _userManager.Users.Where(x => x.UserName != SD.AdminUserName && x.Id==id).Select(member => new MemberAddEditDto
            {
                Id = member.Id,
                UserName = member.UserName,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Roles = string.Join(",", _userManager.GetRolesAsync(member).GetAwaiter().GetResult())
            }).FirstOrDefaultAsync();
            return Ok(members);
        }


        [HttpPost("add-edit-member")]
        public async Task<IActionResult> AddEditMemberr(MemberAddEditDto model)
        {
            User user;
            if (string.IsNullOrEmpty(model.Id))
            {
                // add new 
                if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 0)
                {
                    ModelState.AddModelError("error", "Password must be at least 6 character");
                    return BadRequest(ModelState);
                }
                user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded) return BadRequest(result.Errors);
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Password))
                {
                    if(model.Password.Length < 6)
                    {
                        ModelState.AddModelError("error", "Password must be at least 6 character");
                        return BadRequest(ModelState);
                    }
                }
                if (IsAdminUserId(model.Id))
                {
                    return BadRequest(SD.SuperAdminChangeNotAllow);
                }

                user =await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName=model.FirstName;
                user.UserName=model.UserName;
                user.LastName=model.LastName;

                if (!string.IsNullOrEmpty(model.Password)) { 
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, model.Password);
                }
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            //remove user existing roles
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            foreach (var role in userRoles) {
                var roleToAdd = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == role);
                if (roleToAdd != null) {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            if (string.IsNullOrEmpty(model.Id))
            {
                return Ok(new JsonResult(new { title = "Member Created", message = $"{model.UserName} has been created" }));
            }
            else
            {
                return Ok(new JsonResult(new { title = "Member edited", message = $"{model.UserName} has been updated" }));
            }
        }


        [HttpPut("lock-member/{id}")]
        public async Task<IActionResult> LockMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (IsAdminUserId(user.Id))
            {
                return BadRequest(SD.SuperAdminChangeNotAllow);
            }
            await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddDays(5));
            return NoContent();

        }

        [HttpPut("unlock-member/{id}")]
        public async Task<IActionResult> UnlockMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (IsAdminUserId(user.Id))
            {
                return BadRequest(SD.SuperAdminChangeNotAllow);
            }
            await _userManager.SetLockoutEndDateAsync(user, null);
            return NoContent();

        }

        [HttpDelete("delete-member/{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (IsAdminUserId(user.Id))
            {
                return BadRequest(SD.SuperAdminChangeNotAllow);
            }
            await _userManager.DeleteAsync(user);
            return NoContent();

        }

        [HttpGet("get-application-roles")]
        public async Task<ActionResult<string[]>> GetApplicationRoles()
        {
            return Ok(await _roleManager.Roles.Select(x => x.Name).ToListAsync());

        }

        private bool IsAdminUserId(string userId)
        {
            return _userManager.FindByIdAsync(userId).GetAwaiter().GetResult().UserName.Equals(SD.AdminUserName);
        }
    }
}
