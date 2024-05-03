using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Repositories;

namespace OCR_API.Transactions.FolderTransactions
{
    public class AddFolderTransaction : ITransaction
    {
        private readonly IRepository<Folder> repository;
        private readonly IPasswordHasher<Folder> passwordHasher;
        private readonly int userId;
        private readonly string name;
        private readonly string? iconPath;
        private readonly string? password;
        public Folder Folder { get; private set; }


        public AddFolderTransaction(IRepository<Folder> repository, IPasswordHasher<Folder> passwordHasher, int userId, string name, string? iconPath = null, string? password = null)
        {
            this.repository = repository;
            this.passwordHasher = passwordHasher;
            this.userId = userId;
            this.name = name;
            this.iconPath = iconPath;
            this.password = password;
        }

        public void Execute()
        {
            Folder folderToAdd = new Folder() { UserId = userId, Name = name, IconPath = iconPath };
            if (password is not null)
            {
                var hashedPassword = passwordHasher.HashPassword(folderToAdd, password);
                folderToAdd.PasswordHash = hashedPassword;
            }
            repository.Add(folderToAdd);
            Folder = folderToAdd;
        }
    }
}
