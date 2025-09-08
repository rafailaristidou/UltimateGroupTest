using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventory.Data;
using SimpleInventory.Domain.Entities;

namespace SimpleInventory.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly InventoryDbContext _db;

        public CategoriesController(InventoryDbContext db)
        {
            _db = db;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(categories);
        }

        // GET: /Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("WritePolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (await _db.Categories.AnyAsync(c => c.Name == category.Name))
            {
                ModelState.AddModelError(nameof(Category.Name), "Category name must be unique.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Optional: Delete with constraint check
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("WritePolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();
            if (category.Products.Any())
            {
                ModelState.AddModelError(string.Empty, "Cannot delete a category that has products.");
                return View("Delete", category);
            }
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}


