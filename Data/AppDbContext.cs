using Microsoft.EntityFrameworkCore;
using Demo_Ecommerce.Models;

namespace Demo_Ecommerce.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<WebhookLog> WebhookLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order -> OrderItems relationship
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed initial products
        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                Name = "Wireless Bluetooth Headphones", 
                Description = "Premium noise-canceling wireless headphones with 30-hour battery life", 
                Price = 2999.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=300&h=300&fit=crop", 
                Category = "Electronics",
                Stock = 50
            },
            new Product 
            { 
                Id = 2, 
                Name = "Smart Watch Pro", 
                Description = "Fitness tracking smartwatch with heart rate monitor and GPS", 
                Price = 4599.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=300&h=300&fit=crop", 
                Category = "Electronics",
                Stock = 35
            },
            new Product 
            { 
                Id = 3, 
                Name = "Portable Power Bank 20000mAh", 
                Description = "Fast charging portable charger with dual USB ports", 
                Price = 1299.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5?w=300&h=300&fit=crop", 
                Category = "Electronics",
                Stock = 100
            },
            new Product 
            { 
                Id = 4, 
                Name = "Mechanical Gaming Keyboard", 
                Description = "RGB backlit mechanical keyboard with Cherry MX switches", 
                Price = 3499.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae?w=300&h=300&fit=crop", 
                Category = "Electronics",
                Stock = 25
            },
            new Product 
            { 
                Id = 5, 
                Name = "Wireless Mouse", 
                Description = "Ergonomic wireless mouse with adjustable DPI settings", 
                Price = 899.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=300&h=300&fit=crop", 
                Category = "Electronics",
                Stock = 75
            },
            new Product 
            { 
                Id = 6, 
                Name = "USB-C Hub 7-in-1", 
                Description = "Multi-port USB-C adapter with HDMI, USB 3.0, and SD card reader", 
                Price = 1899.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1625842268584-8f3296236761?w=300&h=300&fit=crop", 
                Category = "Electronics",
                Stock = 60
            },
            new Product 
            { 
                Id = 7, 
                Name = "Bluetooth Speaker", 
                Description = "Waterproof portable Bluetooth speaker with 360° sound", 
                Price = 1599.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=300&h=300&fit=crop", 
                Category = "Electronics",
                Stock = 45
            },
            new Product 
            { 
                Id = 8, 
                Name = "Laptop Stand", 
                Description = "Adjustable aluminum laptop stand for better ergonomics", 
                Price = 999.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=300&h=300&fit=crop", 
                Category = "Accessories",
                Stock = 80
            }
        );
    }
}
