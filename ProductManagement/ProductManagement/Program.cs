using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductManagement.Data;
using ProductManagement.Models.Entities;
using ProductManagement.Services.Token;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================
// CONFIGURATION BASE DE DONNÉES
// =============================
// Ajoute le DbContext (Entity Framework) avec PostgreSQL
// Permet d’injecter ApplicationDbContext dans les controllers/services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// =============================
// CONFIGURATION IDENTITY
// (Gestion Users + Roles + Password hashing)
// =============================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Règles de sécurité des mots de passe
    options.Password.RequireDigit = true;                       // doit contenir un chiffre
    options.Password.RequiredLength = 6;                        // longueur minimale
    options.Password.RequireNonAlphanumeric = false;            // caractères spéciaux non obligatoires
    options.Password.RequireUppercase = true;                   // au moins une majuscule
    options.Password.RequireLowercase = true;                   // au moins une minuscule

    options.User.RequireUniqueEmail = true;                    // Email unique pour chaque utilisateur
})
    .AddEntityFrameworkStores<ApplicationDbContext>()         // Stocke les users/roles dans la base via Entity Framework
    .AddDefaultTokenProviders();                              // Permet la génération de tokens (reset password, confirm email, etc.)

// =============================
// CONFIGURATION JWT
// (Authentification par token)
// =============================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// Récupère la clé secrète pour signer les tokens
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured in appsettings.json");

// Configure le système d’authentification global
builder.Services.AddAuthentication(options =>
{
    // Définit JWT comme méthode d’authentification par défaut
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Configuration du middleware JWT
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                                                                  // Vérifie qui a émis le token
        ValidateAudience = true,                                                                // Vérifie à qui le token est destiné
        ValidateLifetime = true,                                                                // Vérifie expiration
        ValidateIssuerSigningKey = true,                                                        // Vérifie signature du token
        ValidIssuer = jwtSettings["Issuer"],                                                    // Doit correspondre au token
        ValidAudience = jwtSettings["Audience"],                                                // Doit correspondre au token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),         // Clé utilisée pour signer et valider le token
        ClockSkew = TimeSpan.Zero                                                               // Pas de tolérance sur l'expiration (plus strict)
    };
});

// =============================
// INJECTION DE DÉPENDANCE
// (Service personnalisé)
// =============================
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddControllers();

var app = builder.Build();

// =============================
// INITIALISATION DES ROLES
// (Créés automatiquement au démarrage)
// =============================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { RoleUser.ADMIN.ToString(), RoleUser.USER.ToString() };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role)) // Si le role n’existe pas, on le crée
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Force la redirection HTTP to HTTPS
app.UseHttpsRedirection();

app.UseAuthentication();        // Active l’authentification (lecture du token JWT)
app.UseAuthorization();         // Active l’autorisation ([Authorize])

app.MapControllers();           // Mappe les routes des controllers

app.Run();