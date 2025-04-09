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
                .HasForeignKey(p => p.CategoryId);

            // Avoid soft deleted products globally while querying
            entity.HasQueryFilter(p => !p.IsDeleted);
        });
        
        // --- Address ---
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.HasKey(a => a.Id);
            
            entity.Property(a=>a.Street).HasMaxLength(100);
            entity.Property(a=>a.City).HasMaxLength(100);
            entity.Property(a=>a.State).HasMaxLength(100);
            entity.Property(a=>a.Country).HasMaxLength(100);

            entity.HasOne(a => a.customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.customerId);
        });
        
        // --- Category ---
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).HasMaxLength(100);
        });
        
        // --- Order ---
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            
            entity.Property(o=>o.TotalAmount).HasColumnType("Decimal(18,2)");

            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);
        });
        
        // --- OrderItem ---
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(o => o.Id);
            
            // Convert status type enum in OrderItem to string
            entity.Property(o => o.Status).HasConversion<string>();
            entity.Property(o => o.Price).HasColumnType("decimal(18,2)");

            entity.HasOne(o => o.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(o => o.ProductId);
            
            entity.HasOne(o => o.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(o => o.OrderId);
        });
        
        // --- Shopping Cart ---
        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.ToTable("ShoppingCarts");
            entity.HasKey(c => c.Id);

            entity.HasOne(s => s.Customer)
                .WithOne(c => c.ShoppingCart)
                .HasForeignKey<ShoppingCart>(c => c.CustomerId);
        });
        
        // --- ShoppingCart Items ---
        modelBuilder.Entity<ShoppingCartItem>(entity =>
        {
            entity.ToTable("ShoppingCarts");
            entity.HasKey(c => c.Id);
            
            entity.HasOne(s=>s.Cart)
                .WithMany(c=>c.ShoppingCartItems)
                .HasForeignKey(s=>s.CartId);
            
            entity.HasOne(s=>s.Product)
                .WithOne(p=>p.ShoppingCartItem)
                .HasForeignKey<ShoppingCartItem>(s=>s.ProductId);
        });
        
        // --- Wishlist ---
        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.ToTable("Wishlists");
            entity.HasKey(c => c.Id);
            
            entity.HasOne(s => s.Customer)
                .WithOne(c => c.Wishlist)
                .HasForeignKey<Wishlist>(c => c.CustomerId);
        });
        
        // --- Wishlist Items ---
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.ToTable("WishlistItems");
            entity.HasKey(c => c.Id);

            entity.HasOne(s => s.Product)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(s => s.ProductId);
            
            entity.HasOne(w=>w.Wishlist)
                .WithMany(w=>w.WishlistItems)
                .HasForeignKey(w=>w.WishlistId);
        });
    }
}