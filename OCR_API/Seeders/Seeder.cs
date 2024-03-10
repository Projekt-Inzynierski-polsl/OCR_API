using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Entities;
using System.Security.Cryptography;

namespace OCR_API.Seeders
{
    public class Seeder
    {
        private readonly SystemDbContext dbContext;

        public Seeder(SystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void Seed()
        {
            if (dbContext.Database.CanConnect())
            {
                var pendingMigrations = dbContext.Database.GetPendingMigrations();
                if(pendingMigrations != null && pendingMigrations.Any())
                {
                    dbContext.Database.Migrate();
                }

                if(!dbContext.Roles.Any())
                {
                    var roles = GetRoles();
                    dbContext.Roles.AddRange(roles);
                    dbContext.SaveChanges();
                }
            }
        }

        private IEnumerable<Role> GetRoles()
        {
            List<Role> roles = [new Role() { Id = 1, Name = "Admin" }, new Role() { Id = 2, Name = "User" }];
            return roles;
        }
    }
}
