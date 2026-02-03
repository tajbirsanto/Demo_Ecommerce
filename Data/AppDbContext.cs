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
            },
            // Bangladeshi Grocery Items
            new Product 
            { 
                Id = 9, 
                Name = "মুরগির ডিম (১২ পিস)", 
                Description = "তাজা দেশি মুরগির ডিম - ১ ডজন। প্রোটিন সমৃদ্ধ ও পুষ্টিকর।", 
                Price = 180.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=300&h=300&fit=crop", 
                Category = "মুদি দোকান",
                Stock = 200
            },
            new Product 
            { 
                Id = 10, 
                Name = "চাল - মিনিকেট (৫ কেজি)", 
                Description = "প্রিমিয়াম কোয়ালিটি মিনিকেট চাল। সুগন্ধি ও সুস্বাদু।", 
                Price = 450.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=300&h=300&fit=crop", 
                Category = "মুদি দোকান",
                Stock = 150
            },
            new Product 
            { 
                Id = 11, 
                Name = "সয়াবিন তেল (৫ লিটার)", 
                Description = "বিশুদ্ধ সয়াবিন তেল। রান্নার জন্য আদর্শ।", 
                Price = 850.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=300&h=300&fit=crop", 
                Category = "মুদি দোকান",
                Stock = 100
            },
            new Product 
            { 
                Id = 12, 
                Name = "চিনি (১ কেজি)", 
                Description = "সাদা দানাদার চিনি। চা-মিষ্টির জন্য উপযুক্ত।", 
                Price = 120.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=300&h=300&fit=crop", 
                Category = "মুদি দোকান",
                Stock = 300
            },
            new Product 
            { 
                Id = 13, 
                Name = "ডাল - মসুর (১ কেজি)", 
                Description = "লাল মসুর ডাল। উচ্চ প্রোটিন সমৃদ্ধ।", 
                Price = 140.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1613758947307-f3b8f5d80711?w=300&h=300&fit=crop", 
                Category = "মুদি দোকান",
                Stock = 180
            },
            new Product 
            { 
                Id = 14, 
                Name = "পেঁয়াজ (১ কেজি)", 
                Description = "দেশি পেঁয়াজ। তাজা ও সুগন্ধি।", 
                Price = 80.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1618512496248-a07fe83aa8cb?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 500
            },
            new Product 
            { 
                Id = 15, 
                Name = "আলু (২ কেজি)", 
                Description = "ঢাকাই আলু। রান্না ও ভাজির জন্য উপযুক্ত।", 
                Price = 100.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1518977676601-b53f82ber?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 400
            },
            new Product 
            { 
                Id = 16, 
                Name = "মরিচ - কাঁচা (২৫০ গ্রাম)", 
                Description = "ঝাল কাঁচা মরিচ। তাজা ও টাটকা।", 
                Price = 40.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1588252303782-cb80119abd6d?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 250
            }
        );
    }
}
