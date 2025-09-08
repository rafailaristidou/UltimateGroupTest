using Microsoft.EntityFrameworkCore;
using SimpleInventory.Data;
using SimpleInventory.Domain.Entities;

namespace SimpleInventory.Web.Services
{
    public interface IValidationService
    {
        Task<ValidationResult> ValidateProductAsync(Product product, bool isUpdate = false);
        Task<ValidationResult> ValidateCategoryAsync(Category category, bool isUpdate = false);
    }

    public class ValidationService : IValidationService
    {
        private readonly InventoryDbContext _context;

        public ValidationService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<ValidationResult> ValidateProductAsync(Product product, bool isUpdate = false)
        {
            var result = new ValidationResult();

            // Guard against negative price
            if (product.Price <= 0)
            {
                result.AddError(nameof(Product.Price), "Price must be greater than 0");
            }

            // Guard against negative quantity
            if (product.Quantity < 0)
            {
                result.AddError(nameof(Product.Quantity), "Quantity cannot be negative");
            }

            // Guard against duplicate SKU
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Sku == product.Sku && (!isUpdate || p.Id != product.Id));

            if (existingProduct != null)
            {
                result.AddError(nameof(Product.Sku), $"SKU '{product.Sku}' already exists");
            }

            // Validate category exists
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == product.CategoryId);

            if (!categoryExists)
            {
                result.AddError(nameof(Product.CategoryId), "Selected category does not exist");
            }

            // Business rule: SKU format validation
            if (!string.IsNullOrEmpty(product.Sku) && !System.Text.RegularExpressions.Regex.IsMatch(product.Sku, @"^[A-Z0-9]+$"))
            {
                result.AddError(nameof(Product.Sku), "SKU must contain only uppercase letters and numbers");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateCategoryAsync(Category category, bool isUpdate = false)
        {
            var result = new ValidationResult();

            // Guard against duplicate category name
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == category.Name && (!isUpdate || c.Id != category.Id));

            if (existingCategory != null)
            {
                result.AddError(nameof(Category.Name), $"Category name '{category.Name}' already exists");
            }

            return result;
        }
    }

    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public Dictionary<string, List<string>> Errors { get; } = new();

        public void AddError(string field, string message)
        {
            if (!Errors.ContainsKey(field))
            {
                Errors[field] = new List<string>();
            }
            Errors[field].Add(message);
        }

        public void AddErrors(Dictionary<string, List<string>> errors)
        {
            foreach (var error in errors)
            {
                foreach (var message in error.Value)
                {
                    AddError(error.Key, message);
                }
            }
        }
    }
}
