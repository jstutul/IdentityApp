using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class LoginWithExternal
    {
        public string AccessToken { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Provider { get; set; }
    }
}
