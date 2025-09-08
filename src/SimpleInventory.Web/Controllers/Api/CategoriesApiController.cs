using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventory.Data;
using SimpleInventory.Domain.Entities;
using SimpleInventory.Web.Models;

namespace SimpleInventory.Web.Controllers.Api
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly InventoryDbContext _db;

        public CategoriesApiController(InventoryDbContext db)
        {
            _db = db;
        }

        // GET /api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> Get()
        {
            var items = await _db.Categories.OrderBy(c => c.Name)
                .Select(c => new CategoryReadDto { Id = c.Id, Name = c.Name })
                .ToListAsync();
            return Ok(items);
        }

    // POST /api/categories
    [HttpPost]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("WritePolicy")]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            {
                ModelState.AddModelError(nameof(CategoryCreateDto.Name), "Category name must be unique.");
                return ValidationProblem(ModelState);
            }

            var category = new Category { Name = dto.Name };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = category.Id }, new { id = category.Id });
        }

    // DELETE /api/categories/{id}
    [HttpDelete("{id}")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("WritePolicy")]
    public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();
            if (category.Products.Any())
            {
                var problem = new ProblemDetails
                {
                    Title = "Category has products",
                    Detail = "Cannot delete a category that has products.",
                    Status = StatusCodes.Status409Conflict
                };
                return Conflict(problem);
            }
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}


