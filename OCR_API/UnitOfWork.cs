using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API
{
    public interface IUnitOfWork
    {
        IRepository<BlackListToken> BlackListedTokens { get; }
        IRepository<BoundingBox> BoundingBoxes { get; }
        IRepository<ErrorCutFile> ErrorCutFiles { get; }
        IRepository<Folder> Folders { get; }
        IRepository<Note> Notes { get; }
        IRepository<NoteCategory> NoteCategories { get; }
        IRepository<NoteFile> NoteFiles { get; }
        IRepository<NoteLine> NoteLines { get; }
        IRepository<NoteWordError> NoteWordErrors { get; }
        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<UploadedModel> UploadedModels { get; }
        IRepository<UserAction> UserActions { get; }
        IRepository<UserLog> UserLogs { get; }

        IRepository<Shared> Shared { get; }
        IRepository<ShareMode> ShareMode { get; }
        void Commit();
    }
    public class UnitOfWork : IUnitOfWork
    {

        private readonly SystemDbContext dbContext;

        private readonly Repository<BlackListToken> blackListedTokens;
        private readonly Repository<BoundingBox> boundingBoxes;
        private readonly Repository<ErrorCutFile> errorCutFiles;
        private readonly Repository<Folder> folders;
        private readonly Repository<Note> notes;
        private readonly Repository<NoteCategory> noteCategories;
        private readonly Repository<NoteFile> noteFiles;
        private readonly Repository<NoteLine> noteLines;
        private readonly Repository<NoteWordError> noteWordErrors;
        private readonly Repository<UploadedModel> uploadedModels;
        private readonly Repository<User> users;
        private readonly Repository<Role> roles;
        private readonly Repository<UserAction> userActions;
        private readonly Repository<UserLog> userLogs;
        private readonly Repository<Shared> shared;
        private readonly Repository<ShareMode> shareMode;

        public UnitOfWork(SystemDbContext dbContext)
        {
            this.dbContext = dbContext;
            users = new Repository<User>(dbContext);
            roles = new Repository<Role>(dbContext);
            blackListedTokens = new Repository<BlackListToken>(dbContext);
            boundingBoxes = new Repository<BoundingBox>(dbContext);
            errorCutFiles = new Repository<ErrorCutFile>(dbContext);
            folders = new Repository<Folder>(dbContext);
            notes = new Repository<Note>(dbContext);
            noteCategories = new Repository<NoteCategory>(dbContext);
            noteFiles = new Repository<NoteFile>(dbContext);
            noteLines = new Repository<NoteLine>(dbContext);
            noteWordErrors = new Repository<NoteWordError>(dbContext);
            uploadedModels = new Repository<UploadedModel>(dbContext);
            userActions = new Repository<UserAction>(dbContext);
            userLogs = new Repository<UserLog>(dbContext);
            shared = new Repository<Shared>(dbContext);
            shareMode = new Repository<ShareMode>(dbContext);
        }

        public IRepository<User> Users => users;

        public IRepository<Role> Roles => roles;
        public IRepository<BlackListToken> BlackListedTokens => blackListedTokens;

        public IRepository<BoundingBox> BoundingBoxes => boundingBoxes;
        public IRepository<ErrorCutFile> ErrorCutFiles => errorCutFiles;
        public IRepository<Folder> Folders => folders;

        public IRepository<Note> Notes => notes;

        public IRepository<NoteCategory> NoteCategories => noteCategories;
        public IRepository<NoteFile> NoteFiles => noteFiles;

        public IRepository<NoteLine> NoteLines => noteLines;

        public IRepository<NoteWordError> NoteWordErrors => noteWordErrors;

        public IRepository<UploadedModel> UploadedModels => uploadedModels;

        public IRepository<UserAction> UserActions => userActions;

        public IRepository<UserLog> UserLogs => userLogs;

        public IRepository<Shared> Shared => shared;

        public IRepository<ShareMode> ShareMode => shareMode;

        public void Commit()
        {
            dbContext.SaveChanges();
        }
    }
}