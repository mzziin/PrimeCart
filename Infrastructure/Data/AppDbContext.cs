using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Convert status type enum in OrderItem to string
        modelBuilder.Entity<OrderItem>()
            .Property(o => o.Status)
            .HasConversion<string>();

        // --- Customer ---
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.Phone).IsUnique();

            entity.Property(c => c.Name).HasMaxLength(100);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(15);

            // Avoid soft deleted customers globally while querying
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // --- Seller ---
        modelBuilder.Entity<Seller>(entity =>
        {
            entity.ToTable("Sellers");
            entity.HasKey(s => s.Id);

            entity.HasIndex(s => s.Email).IsUnique();
            entity.HasIndex(s => s.StoreName).IsUnique();
            entity.HasIndex(s => s.Phone).IsUnique();

            entity.Property(s => s.Name).HasMaxLength(100);
            entity.Property(s => s.StoreName).HasMaxLength(100);
            entity.Property(s => s.Email).HasMaxLength(200);
            entity.Property(s => s.Phone).HasMaxLength(15);

            // Avoid soft deleted sellers globally while querying
            entity.HasQueryFilter(s => !s.IsDeleted);
        });

        // --- Product ---
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Seller)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SellerId);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(c => c.CategoryId);

            // Avoid soft deleted products globally while querying
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // --- Address ---
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.HasKey(a => a.Id);

            entity.HasOne(a => a.customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.customerId);
        });
    }
}