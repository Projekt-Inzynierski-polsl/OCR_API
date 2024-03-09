using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OCR_API.Exceptions;

namespace OCR_API.DbContexts
{
    public class SystemDbContext : DbContext
    {
        private string connectionString;

        public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }

}
