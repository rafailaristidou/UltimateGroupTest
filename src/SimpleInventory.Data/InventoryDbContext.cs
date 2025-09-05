using Microsoft.EntityFrameworkCore;
using SimpleInventory.Domain.Entities;

namespace SimpleInventory.Data
{
    public class InventoryDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique index for Category.Name
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Unique index for Product.Sku
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            // Seed data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Clothing" },
                new Category { Id = 3, Name = "Books" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Sku = "ELEC001", Name = "Laptop", Price = 999.99m, Quantity = 10, CategoryId = 1, UpdatedAt = DateTime.UtcNow },
                new Product { Id = 2, Sku = "ELEC002", Name = "Monitor", Price = 199.99m, Quantity = 15, CategoryId = 1, UpdatedAt = DateTime.UtcNow },
                new Product { Id = 3, Sku = "CLOT001", Name = "T-Shirt", Price = 19.99m, Quantity = 50, CategoryId = 2, UpdatedAt = DateTime.UtcNow },
                new Product { Id = 4, Sku = "CLOT002", Name = "Jeans", Price = 49.99m, Quantity = 30, CategoryId = 2, UpdatedAt = DateTime.UtcNow },
                new Product { Id = 5, Sku = "BOOK001", Name = "C# Programming", Price = 29.99m, Quantity = 20, CategoryId = 3, UpdatedAt = DateTime.UtcNow }
            );
        }
    }
}