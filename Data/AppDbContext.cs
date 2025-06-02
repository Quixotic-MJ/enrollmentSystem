// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using enrollmentSystem.Models;

namespace enrollmentSystem.Data
{
    public class AppDbContext : DbContext
    {
        // Add this constructor - it's required for dependency injection
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        // Remove any parameterless constructor if you have one
        // public AppDbContext() {} // <- DELETE THIS IF IT EXISTS

        public DbSet<Admin> Admin { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>().ToTable("Admin");
        }
    }
}