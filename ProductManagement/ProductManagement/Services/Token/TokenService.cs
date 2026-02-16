using Microsoft.IdentityModel.Tokens;
using ProductManagement.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProductManagement.Services.Token
{
    public class TokenService : ITokenService
    {

        private readonly IConfiguration _configuration;

        // Injection de dépendance
        // IConfiguration permet de lire les valeurs dans appsettings.json
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Méthode principale appelée après le login
        // Elle génère le token JWT pour l'utilisateur connecté
        public string CreateToken(ApplicationUser user, string role)
        {
            // Récupère les paramètres de configuration
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            // ==============================
            // CLÉ DE SIGNATURE
            // ==============================

            // Crée une clé sécurisée basée sur la SecretKey
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            // Algorithme utilisé pour signer le token
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ==============================
            // CLAIMS (informations stockées dans le token)
            // ==============================
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID unique du token (sécurité supplémentaire)
            };

            // ==============================
            // CRÉATION DU TOKEN
            // ==============================
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            // Convertit l'objet token en string lisible
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
