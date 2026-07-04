using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class RegisterDto
    {
        [Required]
        [StringLength(15,MinimumLength =3,ErrorMessage ="First name must be at least (2),and maximum {1} character")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "last name must be at least (2),and maximum {1} character")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage ="Invalid email address.")]
        public string Email { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Password must be a {2}, and maximum {1} Character")]
        public string Password { get; set; }
    }
}
