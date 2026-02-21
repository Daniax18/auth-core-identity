using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Models.DTO.Product;

namespace ProductManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
        {
            // Vérifie si le modèle reçu respecte les validations (DataAnnotations dans ton DTO).
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Création d’une nouvelle entité Product
            var product = new Models.Entities.Product
            {
                Name = productDto.Name,
                Price = productDto.Price
            };

            // Ajoute l’objet product au DbContext (Entity Framework).
            // À ce stade, rien n’est encore enregistré en base.
            _context.Products.Add(product);

            // Sauvegarde réellement en base de données.
            await _context.SaveChangesAsync();

            // Retourne HTTP 201 Created
            // nameof(GetProduct) → référence la méthode GetProduct
            // new { id = product.Id } → fournit les paramètres nécessaires pour construire l’URL de la ressource créée (ex: /api/product/5)
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
    }
}
