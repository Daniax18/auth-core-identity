using Microsoft.AspNetCore.Identity;

namespace ProductManagement.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsFirstLogin { get; set; } = true; 
    }
}
