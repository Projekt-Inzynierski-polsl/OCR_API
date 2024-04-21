using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Logger;
using System.Security.Cryptography;

namespace OCR_API.Seeders
{
    public class Seeder
    {
        private readonly SystemDbContext dbContext;
        private readonly IUnitOfWork unitOfWork;

        public Seeder(SystemDbContext dbContext, IUnitOfWork unitOfWork)
        {
            this.dbContext = dbContext;
            this.unitOfWork = unitOfWork;
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

                if(!unitOfWork.Roles.Entity.Any())
                {
                    var roles = GetRoles();
                    unitOfWork.Roles.Entity.AddRange(roles);
                    unitOfWork.Commit();
                }

                if (!unitOfWork.Roles.Entity.Any())
                {
                    var modes = GetModes();
                    unitOfWork.ShareMode.Entity.AddRange(modes);
                    unitOfWork.Commit();
                }

                if (!unitOfWork.UserActions.Entity.Any())
                {
                    EUserAction[] userActions = (EUserAction[])Enum.GetValues(typeof(EUserAction));

                    foreach (var action in userActions)
                    {
                        if((int)action > 0) unitOfWork.UserActions.Add(new UserAction() { Id = (int)action, Name = action.ToString() });
                    }
                    unitOfWork.Commit();

                }
            }
        }

        private IEnumerable<Role> GetRoles()
        {
            List<Role> roles = [new Role() { Name = "Admin" }, new Role() { Name = "User" }];
            return roles;
        }

        private IEnumerable<ShareMode> GetModes()
        {
            List<ShareMode> modes = [new ShareMode() { Name = "View" }, new ShareMode() { Name = "Edit" }];
            return modes;
        }
    }
}
