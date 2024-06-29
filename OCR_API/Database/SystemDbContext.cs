using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using System.Reflection;

namespace OCR_API.DbContexts
{
    public class SystemDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly()
            );
        }
    }
}