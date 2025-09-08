using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using SimpleInventory.Data;
using SimpleInventory.Domain.Entities;
using SimpleInventory.Web.Controllers;

namespace SimpleInventory.Tests;

public class ControllerTests
{
    private static InventoryDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var ctx = new InventoryDbContext(options);
        return ctx;
    }

    [Fact]
    public async Task ProductCreationValidatorRejectsNegativePrice()
    {
        var product = new Product
        {
            Sku = "NEG001",
            Name = "Negative Price",
            Price = -5m,
            Quantity = 1,
            CategoryId = 1
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, new ValidationContext(product), validationResults, true);

        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public async Task ProductCreationControllerRejectsDuplicateSku()
    {
        var dbName = nameof(ProductCreationControllerRejectsDuplicateSku);
        using var ctx = CreateInMemoryContext(dbName);

        // seed a category and a product with SKU DUP1
        ctx.Categories.Add(new Category { Id = 1, Name = "SeedCat" });
        ctx.Products.Add(new Product { Id = 1, Sku = "DUP1", Name = "Seed", Price = 1m, Quantity = 1, CategoryId = 1 });
        await ctx.SaveChangesAsync();

        var controller = new ProductsController(ctx);

        var newProduct = new Product { Sku = "DUP1", Name = "New", Price = 2m, Quantity = 5, CategoryId = 1 };

        var result = await controller.Create(newProduct);

        // Controller should add a model state error for Sku and return the Create view (ViewResult)
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Product.Sku)));
        var viewResult = Assert.IsType<ViewResult>(result);
        // When invalid, the controller repopulates categories and returns the product back to the view
        Assert.Equal(newProduct, viewResult.Model);
    }

    [Fact]
    public async Task ProductsControllerIndexFiltersByQueryAndCategory()
    {
        var dbName = nameof(ProductsControllerIndexFiltersByQueryAndCategory);
        using var ctx = CreateInMemoryContext(dbName);

        // seed categories
        ctx.Categories.AddRange(
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Clothing" }
        );

        // seed products
        ctx.Products.AddRange(
            new Product { Id = 1, Sku = "ELEC001", Name = "Laptop", Price = 1000m, Quantity = 5, CategoryId = 1 },
            new Product { Id = 2, Sku = "ELEC002", Name = "Monitor", Price = 200m, Quantity = 8, CategoryId = 1 },
            new Product { Id = 3, Sku = "CLOT001", Name = "T-Shirt", Price = 20m, Quantity = 50, CategoryId = 2 }
        );
        await ctx.SaveChangesAsync();

        var controller = new ProductsController(ctx);

        // Search for 'Laptop'
        var result = await controller.Index("Laptop", null, null);
        var vr = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(vr.Model);
        Assert.Single(model);
        Assert.Equal("Laptop", model.First().Name);

        // Filter by category 1 (Electronics)
        var result2 = await controller.Index(null, 1, null);
        var vr2 = Assert.IsType<ViewResult>(result2);
        var model2 = Assert.IsAssignableFrom<IEnumerable<Product>>(vr2.Model);
        Assert.Equal(2, model2.Count());
    }

    [Fact]
    public async Task CategoryDeletionPreventsWhenProductsExist()
    {
        var dbName = nameof(CategoryDeletionPreventsWhenProductsExist);
        using var ctx = CreateInMemoryContext(dbName);

        var category = new Category { Id = 1, Name = "HasProducts" };
        ctx.Categories.Add(category);
        ctx.Products.Add(new Product { Id = 1, Sku = "P1", Name = "Prod1", Price = 1m, Quantity = 1, CategoryId = 1 });
        await ctx.SaveChangesAsync();

        var controller = new CategoriesController(ctx);

        var result = await controller.DeleteConfirmed(1);

        // Should not delete and should return the Delete view with model and model state error
        var vr = Assert.IsType<ViewResult>(result);
        Assert.Equal("Delete", vr.ViewName);
        Assert.True(controller.ModelState.ErrorCount > 0);
        Assert.Contains(controller.ModelState[string.Empty].Errors, e => e.ErrorMessage.Contains("Cannot delete a category"));
    }
}
