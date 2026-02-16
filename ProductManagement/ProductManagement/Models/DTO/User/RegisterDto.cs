using ProductManagement.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Models.DTO.User
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = RoleUser.USER.ToString();
    }
}
