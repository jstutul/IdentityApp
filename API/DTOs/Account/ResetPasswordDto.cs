using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Password must be a (3), and maximum (15) Character")]
        public string NewPassword { get; set; }
    }
}
