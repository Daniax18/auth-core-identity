using ProductManagement.Models.Entities;

namespace ProductManagement.Services.Token
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, string role);
    }
}
