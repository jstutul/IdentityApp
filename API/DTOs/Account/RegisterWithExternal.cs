using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class RegisterWithExternal
    {
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "First name must be at least (3),and maximum (15) character")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "last name must be at least (2),and maximum {1} character")]
        public string LastName { get; set; }
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Provider { get; set; }
    }
}
