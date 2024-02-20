using ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace ecommerce.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories {  get; set; }
        public DbSet<User> Users {  get; set; }
        public DbSet<Profile> Profiles {  get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies(false);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RolePermission>()
                .HasKey(bc => new { bc.RoleId, bc.PermissionId });

            builder.Entity<Category>()
                .HasMany(e => e.Products)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .IsRequired(false);

            builder.Entity<Product>()
                .HasMany(e => e.Images)
                .WithOne(e => e.product)
                .HasForeignKey(e => e.ProductId);

            builder.Entity<Product>()
                .HasOne(e => e.Category);

            builder.Entity<RolePermission>()
               .HasOne(bc => bc.Role)
               .WithMany(b => b.RolePermissions)
               .HasForeignKey(bc => bc.RoleId);

            builder.Entity<RolePermission>()
                .HasOne(bc => bc.Permission)
                .WithMany(c => c.RolePermissions)
                .HasForeignKey(bc => bc.PermissionId);
        }
    }
}
