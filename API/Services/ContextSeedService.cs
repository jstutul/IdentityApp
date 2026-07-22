using API.Data;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Services
{
    public class ContextSeedService
    {
        private readonly Context _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public ContextSeedService(Context context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager= userManager;
            _roleManager= roleManager;
        }


        public async Task initializeContextAsync()
        {
            if (_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Count() > 0)
            {
                await _context.Database.MigrateAsync();
            }

            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.ManagerRole });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.PlayerRole });
            }

            if (!_userManager.Users.AnyAsync().GetAwaiter().GetResult())
            {
                var admin = new User
                {
                    FirstName = "admin",
                    LastName = "jackson",
                    UserName = SD.AdminUserName,
                    Email = SD.AdminUserName,
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin,"123456");
                await _userManager.AddToRolesAsync(admin, new[] { SD.AdminRole, SD.ManagerRole, SD.PlayerRole });
                await _userManager.AddClaimsAsync(admin,new Claim[]
                {
                    new Claim(ClaimTypes.Email,admin.Email),
                    new Claim(ClaimTypes.Surname,admin.LastName)
                });

                var manager = new User
                {
                    FirstName = "manager",
                    LastName = "mobin",
                    UserName = "manager@gmail.com",
                    Email = "managern@gmail.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(manager, "123456");
                await _userManager.AddToRolesAsync(manager, new[] { SD.ManagerRole });
                await _userManager.AddClaimsAsync(manager, new Claim[]
                {
                    new Claim(ClaimTypes.Email,manager.Email),
                    new Claim(ClaimTypes.Surname,manager.LastName)
                });

                var player = new User
                {
                    FirstName = "player",
                    LastName = "playerjackson",
                    UserName = "player@gmail.com",
                    Email = "player@gmail.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(player, "123456");
                await _userManager.AddToRolesAsync(player, new[] { SD.PlayerRole });
                await _userManager.AddClaimsAsync(player, new Claim[]
                {
                    new Claim(ClaimTypes.Email,player.Email),
                    new Claim(ClaimTypes.Surname,player.LastName)
                });

                var vipplayer = new User
                {
                    FirstName = "vipplayer",
                    LastName = "vipplayer",
                    UserName = "vipplayer@gmail.com",
                    Email = "vipplayer@gmail.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(vipplayer, "123456");
                await _userManager.AddToRolesAsync(vipplayer, new[] { SD.PlayerRole });
                await _userManager.AddClaimsAsync(vipplayer, new Claim[]
                {
                    new Claim(ClaimTypes.Email,vipplayer.Email),
                    new Claim(ClaimTypes.Surname,vipplayer.LastName)
                });
            }
        }
    }
}
