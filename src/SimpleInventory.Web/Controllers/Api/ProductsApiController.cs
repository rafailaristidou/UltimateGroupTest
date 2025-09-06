using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventory.Data;
using SimpleInventory.Domain.Entities;
using SimpleInventory.Web.Models;

namespace SimpleInventory.Web.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly InventoryDbContext _db;

        public ProductsApiController(InventoryDbContext db)
        {
            _db = db;
        }

        // GET /api/products
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductReadDto>>> Get([FromQuery] string? q, [FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var lowered = q.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowered) || p.Sku.ToLower().Contains(lowered));
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductReadDto
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .ToListAsync();

            return Ok(new PagedResult<ProductReadDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }

        // GET /api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductReadDto>> GetById(int id)
        {
            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            var dto = new ProductReadDto
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name
            };
            return Ok(dto);
        }

        // POST /api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (await _db.Products.AnyAsync(p => p.Sku == dto.Sku))
            {
                ModelState.AddModelError(nameof(ProductCreateDto.Sku), "Sku must be unique.");
                return ValidationProblem(ModelState);
            }
            var product = new Product
            {
                Sku = dto.Sku,
                Name = dto.Name,
                Price = dto.Price,
                Quantity = dto.Quantity,
                CategoryId = dto.CategoryId,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { id = product.Id });
        }

        // PUT /api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            if (await _db.Products.AnyAsync(p => p.Sku == dto.Sku && p.Id != id))
            {
                ModelState.AddModelError(nameof(ProductUpdateDto.Sku), "Sku must be unique.");
                return ValidationProblem(ModelState);
            }

            product.Sku = dto.Sku;
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}


