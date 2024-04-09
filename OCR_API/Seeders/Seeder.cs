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

                if (!unitOfWork.UserActions.Entity.Any())
                {
                    Dictionary<EUserAction, string> userActions = new Dictionary<EUserAction, string>
                    {
                        { EUserAction.Registration, "Registration" },
                        { EUserAction.Login, "Login" },
                        { EUserAction.RefreshToken, "RefreshToken" },
                        { EUserAction.AddFolder, "AddFolder" },
                        { EUserAction.RemoveFolder, "RemoveFolder" },
                        { EUserAction.EditFolder, "EditFolder" },
                        { EUserAction.AddNote, "AddNote" },
                        { EUserAction.RemoveNote, "RemoveNote" },
                        { EUserAction.EditNote, "EditNote" },
                        { EUserAction.ChangeNoteFolder, "ChangeNoteFolder" },
                        { EUserAction.ReportError, "ReportError" },
                        { EUserAction.EditUser, "EditUser" },
                        { EUserAction.LogoutUser, "LogoutUser" },
                        { EUserAction.DeleteError, "DeleteError" },
                        { EUserAction.DownloadErrors, "DownloadErrors" },
                        { EUserAction.ClearErrorTable, "ClearErrorTable" },
                        { EUserAction.AddCategory, "AddCategory" },
                        { EUserAction.RemoveCategory, "RemoveCategory" },
                        { EUserAction.UpdateCategory, "UpdateCategory" }
                    };

                    foreach(var action in userActions)
                    {
                        unitOfWork.UserActions.Add(new UserAction() { Id = (int)action.Key, Name = action.Value });
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
    }
}
