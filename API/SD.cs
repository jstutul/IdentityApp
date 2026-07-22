using API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API
{
    public static class SD
    {
        public const string Facebook = "facebook";
        public const string Google = "google";
        // Roles
        public const string AdminRole = "Admin";
        public const string ManagerRole = "Manager";
        public const string PlayerRole = "Player";

        public const string AdminUserName = "admin@gmail.com";
        public const string SuperAdminChangeNotAllow = "Super Admin change is not allowed";

        public const int MaxmimumLoginAttemps = 3;

        public static bool VIPPolicy(AuthorizationHandlerContext context)
        {
            if(context.User.IsInRole(PlayerRole) && context.User.HasClaim(c=>c.Type == ClaimTypes.Email && c.Value.Contains("vip")))
            {
                return true;
            }
            return false;
        }
    }
}
