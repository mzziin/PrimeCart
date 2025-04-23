using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
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

        // --- User ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            // setup Id as a non-clustered pk
            entity.HasKey(e => e.Id).IsClustered(false);

            /*
            Make RowId a unique and clustered index for
            efficient inserts and improved write performance.
            This also reduces index fragmentation caused by random Guid values.
            */
            entity.Property(p => p.RowId).UseIdentityColumn();
            entity.HasIndex(p => p.RowId)
                .IsUnique()
                .IsClustered();

            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);

            // Convert Role type enum to string
            entity.Property(p => p.Role)
                .IsRequired()
                .HasConversion<string>();

            // Avoid soft deleted users globally while querying
            // entity.HasQueryFilter(u => u.IsActive);
        });

        // --- Customer ---
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.Id).IsClustered(false);
            entity.Property(c => c.RowId).UseIdentityColumn();
            entity.HasIndex(c => c.RowId)
                .IsUnique()
                .IsClustered();

            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Phone).IsRequired().HasMaxLength(15);
            entity.HasIndex(c => c.Phone).IsUnique();

            entity.HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId);
        });

        // --- Seller ---
        modelBuilder.Entity<Seller>(entity =>
        {
            entity.ToTable("Sellers");
            entity.HasKey(s => s.Id).IsClustered(false);
            entity.Property(s => s.RowId).UseIdentityColumn();
            entity.HasIndex(s=>s.RowId)
                .IsUnique()
                .IsClustered();
            
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.StoreName).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => s.StoreName).IsUnique();
            entity.Property(s => s.Phone).IsRequired().HasMaxLength(15);
            entity.HasIndex(s => s.Phone).IsUnique();
            
            entity.HasOne(s => s.User)
                .WithOne(u => u.Seller)
                .HasForeignKey<Seller>(s => s.UserId);
        });

        // --- Product ---
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            
            entity.HasKey(p => p.Id).IsClustered(false);
            entity.Property(p => p.RowId).UseIdentityColumn();
            entity.HasIndex(p => p.RowId)
                .IsUnique()
                .IsClustered();

            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Seller)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SellerId);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
        });

        // --- Address ---
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            
            entity.HasKey(a => a.Id).IsClustered(false);
            entity.Property(a => a.RowId).UseIdentityColumn();
            entity.HasIndex(a => a.RowId)
                .IsUnique()
                .IsClustered();

            entity.Property(a => a.Street).IsRequired().HasMaxLength(100);
            entity.Property(a => a.City).IsRequired().HasMaxLength(100);
            entity.Property(a => a.State).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Country).IsRequired().HasMaxLength(100);

            entity.HasOne(a => a.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CustomerId);
            
            entity.HasQueryFilter(a => a.Customer.User.IsActive);
        });

        // --- Category ---
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            entity.HasKey(c => c.Id).IsClustered(false);
            entity.Property(c => c.RowId).UseIdentityColumn();
            entity.HasIndex(c => c.RowId)
                .IsUnique()
                .IsClustered();

            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        // --- Order ---
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            
            entity.HasKey(o => o.Id).IsClustered(false);
            entity.Property(o => o.RowId).UseIdentityColumn();
            entity.HasIndex(o => o.RowId)
                .IsUnique()
                .IsClustered();

            entity.Property(o => o.TotalAmount).HasColumnType("Decimal(18,2)");

            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);
        });

        // --- OrderItem ---
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            
            entity.HasKey(o => o.Id).IsClustered(false);
            entity.Property(o => o.RowId).UseIdentityColumn();
            entity.HasIndex(o => o.RowId)
                .IsUnique()
                .IsClustered();

            // Convert status type enum in OrderItem to string
            entity.Property(o => o.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(o => o.Price).HasColumnType("decimal(18,2)");
            entity.Property(o => o.DeliveredAt).IsRequired(false);

            entity.HasOne(o => o.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(o => o.OrderId);
        });

        // --- Shopping Cart ---
        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.ToTable("ShoppingCarts");

            entity.HasKey(c => c.Id).IsClustered(false);
            entity.Property(s => s.RowId).UseIdentityColumn();
            entity.HasIndex(s => s.RowId)
                .IsUnique()
                .IsClustered();

            entity.HasOne(s => s.Customer)
                .WithOne(c => c.ShoppingCart)
                .HasForeignKey<ShoppingCart>(c => c.CustomerId);
        });

        // --- ShoppingCart Items ---
        modelBuilder.Entity<ShoppingCartItem>(entity =>
        {
            entity.ToTable("ShoppingCartItems");

            entity.HasKey(c => c.Id).IsClustered(false);
            entity.Property(s => s.RowId).UseIdentityColumn();
            entity.HasIndex(s => s.RowId)
                .IsUnique()
                .IsClustered();

            entity.HasOne(s => s.Cart)
                .WithMany(c => c.ShoppingCartItems)
                .HasForeignKey(s => s.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Product)
                .WithOne(p => p.ShoppingCartItem)
                .HasForeignKey<ShoppingCartItem>(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Wishlist ---
        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.ToTable("Wishlists");

            entity.HasKey(w => w.Id).IsClustered(false);
            entity.Property(w => w.RowId).UseIdentityColumn();
            entity.HasIndex(w => w.RowId)
                .IsUnique()
                .IsClustered();

            entity.HasOne(s => s.Customer)
                .WithOne(c => c.Wishlist)
                .HasForeignKey<Wishlist>(c => c.CustomerId);
        });

        // --- Wishlist Items ---
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.ToTable("WishlistItems");

            entity.HasKey(w => w.Id).IsClustered(false);
            entity.Property(w => w.RowId).UseIdentityColumn();
            entity.HasIndex(w => w.RowId)
                .IsUnique()
                .IsClustered();

            entity.HasOne(w => w.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.Wishlist)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(w => w.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}