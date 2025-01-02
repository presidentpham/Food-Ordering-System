using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties for your application-specific models
        public DbSet<Product> Products { get; set; }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<PaymentOptions> PaymentOptions { get; set; }
        public DbSet<SyncIds> SyncIds { get; set; }

        public DbSet<Admins> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base method to ensure Identity configurations are applied
            base.OnModelCreating(modelBuilder);

            // Configure SyncIdModel relationships and properties
            modelBuilder.Entity<SyncIds>()
                .HasKey(s => s.Id); // Configure primary key if necessary
            modelBuilder.Entity<Admins>()
                .HasKey(s => s.Password); // Configure primary key if necessary
            // Configure relationships and behaviors for ShoppingCart and ShoppingCartItem
            modelBuilder.Entity<ShoppingCart>()
                .HasMany(sc => sc.Items)
                .WithOne(si => si.ShoppingCart)
                .HasForeignKey(si => si.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade); // Adjust delete behavior as needed

            modelBuilder.Entity<ShoppingCartItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.ShoppingCartItems) // Ensure to configure reverse navigation in Product if needed
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Adjust delete behavior as needed

            modelBuilder.Entity<Product>().ToTable("Products");
            // If you need to add additional configuration for IdentityUser, do it here.
            // For example:
            // modelBuilder.Entity<IdentityUser>()
            //     .ToTable("AspNetUsers");
        }
    }
}
