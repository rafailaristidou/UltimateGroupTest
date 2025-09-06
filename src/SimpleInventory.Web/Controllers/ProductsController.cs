using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleInventory.Data;
using SimpleInventory.Domain.Entities;

namespace SimpleInventory.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly InventoryDbContext _db;

        public ProductsController(InventoryDbContext db)
        {
            _db = db;
        }

        // GET: /Products
        public async Task<IActionResult> Index(string? q, int? categoryId, string? sort, int page = 1, int pageSize = 10)
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

            // Handle sorting - SQLite doesn't support decimal in ORDER BY, so we'll sort in memory for price
            var total = await query.CountAsync();
            var allItems = await query.ToListAsync();
            
            var sortedItems = sort switch
            {
                "name_desc" => allItems.OrderByDescending(p => p.Name),
                "price" => allItems.OrderBy(p => p.Price),
                "price_desc" => allItems.OrderByDescending(p => p.Price),
                _ => allItems.OrderBy(p => p.Name),
            };
            
            var items = sortedItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Query = q;
            ViewBag.CategoryId = categoryId;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

            return View(items);
        }

        // GET: /Products/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (await _db.Products.AnyAsync(p => p.Sku == product.Sku))
            {
                ModelState.AddModelError(nameof(Product.Sku), "Sku must be unique.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(product);
            }

            product.UpdatedAt = DateTime.UtcNow;
            _db.Add(product);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            await PopulateCategoriesAsync();
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            if (await _db.Products.AnyAsync(p => p.Sku == product.Sku && p.Id != id))
            {
                ModelState.AddModelError(nameof(Product.Sku), "Sku must be unique.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(product);
            }

            try
            {
                var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (existing == null) return NotFound();

                existing.Sku = product.Sku;
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.Quantity = product.Quantity;
                existing.CategoryId = product.CategoryId;
                existing.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Problem("Concurrency error while updating product.");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync()
        {
            ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        }
    }
}


