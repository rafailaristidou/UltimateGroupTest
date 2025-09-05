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

            // Seed initial data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Books" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Sku = "ELEC-001",
                    Name = "Smartphone",
                    Price = 499.99m,
                    Quantity = 10,
                    CategoryId = 1,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Sku = "BOOK-001",
                    Name = "C# Programming",
                    Price = 29.99m,
                    Quantity = 50,
                    CategoryId = 2,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}