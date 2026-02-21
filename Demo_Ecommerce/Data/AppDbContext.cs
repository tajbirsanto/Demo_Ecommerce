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
            },
            // ===== মাছ =====
            new Product 
            { 
                Id = 17, 
                Name = "ইলিশ মাছ (১ কেজি)", 
                Description = "পদ্মার তাজা ইলিশ মাছ। বাঙালির প্রিয় মাছ।", 
                Price = 1200.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=300&h=300&fit=crop", 
                Category = "মাছ",
                Stock = 30
            },
            new Product 
            { 
                Id = 18, 
                Name = "রুই মাছ (১ কেজি)", 
                Description = "বড় রুই মাছ। কারি ও ভাজার জন্য আদর্শ।", 
                Price = 350.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=300&h=300&fit=crop", 
                Category = "মাছ",
                Stock = 50
            },
            new Product 
            { 
                Id = 19, 
                Name = "চিংড়ি মাছ (৫০০ গ্রাম)", 
                Description = "বাগদা চিংড়ি। ভাপা ও কারির জন্য উপযুক্ত।", 
                Price = 650.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?w=300&h=300&fit=crop", 
                Category = "মাছ",
                Stock = 40
            },
            // ===== মশলা =====
            new Product 
            { 
                Id = 20, 
                Name = "হলুদ গুঁড়া (২০০ গ্রাম)", 
                Description = "বিশুদ্ধ হলুদ গুঁড়া। রান্নায় রং ও স্বাদ আনে।", 
                Price = 60.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1615485500704-8e990f9900f7?w=300&h=300&fit=crop", 
                Category = "মশলা",
                Stock = 300
            },
            new Product 
            { 
                Id = 21, 
                Name = "মরিচ গুঁড়া (২০০ গ্রাম)", 
                Description = "কাশ্মীরি মরিচ গুঁড়া। ঝাল ও রং দুটোই পারফেক্ট।", 
                Price = 70.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=300&h=300&fit=crop", 
                Category = "মশলা",
                Stock = 250
            },
            new Product 
            { 
                Id = 22, 
                Name = "জিরা (১০০ গ্রাম)", 
                Description = "আস্ত জিরা। বিরিয়ানি ও তরকারিতে সুগন্ধ আনে।", 
                Price = 55.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1599909533681-74084161b163?w=300&h=300&fit=crop", 
                Category = "মশলা",
                Stock = 200
            },
            new Product 
            { 
                Id = 23, 
                Name = "ধনে গুঁড়া (২০০ গ্রাম)", 
                Description = "খাঁটি ধনে গুঁড়া। মশলার মূল উপকরণ।", 
                Price = 50.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1532336414038-cf19250c5757?w=300&h=300&fit=crop", 
                Category = "মশলা",
                Stock = 280
            },
            new Product 
            { 
                Id = 24, 
                Name = "গরম মশলা (১০০ গ্রাম)", 
                Description = "এলাচ, দারুচিনি, লবঙ্গ মিশ্রিত গরম মশলা।", 
                Price = 90.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=300&h=300&fit=crop", 
                Category = "মশলা",
                Stock = 150
            },
            // ===== ফল =====
            new Product 
            { 
                Id = 25, 
                Name = "আম - হিমসাগর (১ কেজি)", 
                Description = "রাজশাহীর হিমসাগর আম। মিষ্টি ও রসালো।", 
                Price = 280.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1553279768-865429fa0078?w=300&h=300&fit=crop", 
                Category = "ফল",
                Stock = 100
            },
            new Product 
            { 
                Id = 26, 
                Name = "কলা - সাগর (১ ডজন)", 
                Description = "পাকা সাগর কলা। পুষ্টিকর ও সুস্বাদু।", 
                Price = 60.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?w=300&h=300&fit=crop", 
                Category = "ফল",
                Stock = 200
            },
            new Product 
            { 
                Id = 27, 
                Name = "মালটা (১ কেজি)", 
                Description = "সিলেটি মালটা। ভিটামিন C সমৃদ্ধ।", 
                Price = 180.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1547514701-42782101795e?w=300&h=300&fit=crop", 
                Category = "ফল",
                Stock = 120
            },
            new Product 
            { 
                Id = 28, 
                Name = "লিচু (১ কেজি)", 
                Description = "দিনাজপুরের মিষ্টি লিচু। সিজনাল ফল।", 
                Price = 200.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1622254951691-11b0e5da45ff?w=300&h=300&fit=crop", 
                Category = "ফল",
                Stock = 80
            },
            // ===== দুগ্ধ =====
            new Product 
            { 
                Id = 29, 
                Name = "দুধ - পাস্তুরাইজড (১ লি.)", 
                Description = "ফার্ম ফ্রেশ পাস্তুরাইজড দুধ। ক্যালসিয়াম সমৃদ্ধ।", 
                Price = 95.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1550583724-b2692b85b150?w=300&h=300&fit=crop", 
                Category = "দুগ্ধ",
                Stock = 150
            },
            new Product 
            { 
                Id = 30, 
                Name = "ঘি - খাঁটি (২৫০ গ্রাম)", 
                Description = "গাভীর দুধের খাঁটি ঘি। বিরিয়ানি ও পোলাওয়ে অসাধারণ।", 
                Price = 350.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1631452180519-c014fe946bc7?w=300&h=300&fit=crop", 
                Category = "দুগ্ধ",
                Stock = 60
            },
            new Product 
            { 
                Id = 31, 
                Name = "দই - মিষ্টি (৫০০ গ্রাম)", 
                Description = "বগুড়ার বিখ্যাত মিষ্টি দই। হ্যান্ডমেইড।", 
                Price = 120.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=300&h=300&fit=crop", 
                Category = "দুগ্ধ",
                Stock = 80
            },
            // ===== মিষ্টি ও নাস্তা =====
            new Product 
            { 
                Id = 32, 
                Name = "রসগোল্লা (১ কেজি)", 
                Description = "পুরান ঢাকার নরম রসগোল্লা। ১২ পিস।", 
                Price = 250.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1666190050276-5e781c984036?w=300&h=300&fit=crop", 
                Category = "মিষ্টি",
                Stock = 50
            },
            new Product 
            { 
                Id = 33, 
                Name = "সন্দেশ - নলেন গুড়ের", 
                Description = "খেজুরের গুড়ের সন্দেশ। শীতকালীন স্পেশাল।", 
                Price = 300.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1551024506-0bccd828d307?w=300&h=300&fit=crop", 
                Category = "মিষ্টি",
                Stock = 40
            },
            new Product 
            { 
                Id = 34, 
                Name = "চানাচুর (৫০০ গ্রাম)", 
                Description = "মসলাদার বোম্বে চানাচুর। চায়ের সাথে পারফেক্ট।", 
                Price = 80.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1599490659213-e2b9527bd087?w=300&h=300&fit=crop", 
                Category = "নাস্তা",
                Stock = 200
            },
            new Product 
            { 
                Id = 35, 
                Name = "পিঠা - ভাপা (১০ পিস)", 
                Description = "গুড় ও নারকেল দিয়ে তৈরি ভাপা পিঠা।", 
                Price = 150.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1558961363-fa8fdf82db35?w=300&h=300&fit=crop", 
                Category = "নাস্তা",
                Stock = 30
            },
            // ===== আরও সবজি =====
            new Product 
            { 
                Id = 36, 
                Name = "টমেটো (১ কেজি)", 
                Description = "টাটকা লাল টমেটো। সালাদ ও রান্না দুটোতেই চমৎকার।", 
                Price = 60.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1546470427-0d4db154ceb8?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 300
            },
            new Product 
            { 
                Id = 37, 
                Name = "বেগুন (৫০০ গ্রাম)", 
                Description = "দেশি বেগুন। ভর্তা ও ভাজির জন্য আদর্শ।", 
                Price = 35.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1615484477778-ca3b77940c25?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 200
            },
            new Product 
            { 
                Id = 38, 
                Name = "ফুলকপি (১ পিস)", 
                Description = "বড় সাইজের তাজা ফুলকপি। স্বাস্থ্যকর সবজি।", 
                Price = 45.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1568702846914-96b305d2aaeb?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 150
            },
            new Product 
            { 
                Id = 39, 
                Name = "লাউ (১ পিস)", 
                Description = "বড় সাইজের লাউ। ডাল ও ভাজি দুটোতেই সুস্বাদু।", 
                Price = 50.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1590868309235-ea34bed7bd7f?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 100
            },
            new Product 
            { 
                Id = 40, 
                Name = "শিম (৫০০ গ্রাম)", 
                Description = "দেশি শিম। শীতকালের জনপ্রিয় সবজি।", 
                Price = 40.00m, 
                ImageUrl = "https://images.unsplash.com/photo-1551462147-ff29053bfc14?w=300&h=300&fit=crop", 
                Category = "সবজি",
                Stock = 180
            }
        );
    }
}
