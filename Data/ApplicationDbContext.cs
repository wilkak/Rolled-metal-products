using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Rolled_metal_products.Models;

namespace Rolled_metal_products.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {
            
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<ApplicationType> ApplicationType { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }
}
