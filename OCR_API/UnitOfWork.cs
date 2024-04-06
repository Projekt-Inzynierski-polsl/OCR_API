using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Repositories;


public interface IUnitOfWork
{
    int UserId { get; set; }
    IRepository<BlackListToken> BlackListedTokens { get; }
    IRepository<BoundingBox> BoundingBoxes { get; }
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
    void Commit();
}
public class UnitOfWork : IUnitOfWork
{

    private readonly SystemDbContext dbContext;

    public int UserId { get; set; }
    private Repository<BlackListToken> blackListedTokens;
    private Repository<BoundingBox> boundingBoxes;
    private Repository<Folder> folders;
    private Repository<Note> notes;
    private Repository<NoteCategory> noteCategories;
    private Repository<NoteFile> noteFiles;
    private Repository<NoteLine> noteLines;
    private Repository<NoteWordError> noteWordErrors;
    private Repository<UploadedModel> uploadedModels;
    private Repository<User> users;
    private Repository<Role> roles;
    private Repository<UserAction> userActions;
    private Repository<UserLog> userLogs;


    public UnitOfWork(SystemDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IRepository<User> Users
    {
        get
        {
            return users ??
                (users = new Repository<User>(dbContext));
        }
    }

    public IRepository<Role> Roles
    {
        get
        {
            return roles ??
                (roles = new Repository<Role>(dbContext));
        }
    }
    public IRepository<BlackListToken> BlackListedTokens
    {
        get
        {
            return blackListedTokens ??
                (blackListedTokens = new Repository<BlackListToken>(dbContext));
        }
    }

    public IRepository<BoundingBox> BoundingBoxes
    {
        get
        {
            return boundingBoxes ??
                (boundingBoxes = new Repository<BoundingBox>(dbContext));
        }
    }
    public IRepository<Folder> Folders
    {
        get
        {
            return folders ??
                (folders = new Repository<Folder>(dbContext));
        }
    }

    public IRepository<Note> Notes
    {
        get
        {
            return notes ??
                (notes = new Repository<Note>(dbContext));
        }
    }

    public IRepository<NoteCategory> NoteCategories
    {
        get
        {
            return noteCategories ??
                (noteCategories = new Repository<NoteCategory>(dbContext));
        }
    }
    public IRepository<NoteFile> NoteFiles
    {
        get
        {
            return noteFiles ??
                (noteFiles = new Repository<NoteFile>(dbContext));
        }
    }

    public IRepository<NoteLine> NoteLines
    {
        get
        {
            return noteLines ??
                (noteLines = new Repository<NoteLine>(dbContext));
        }
    }

    public IRepository<NoteWordError> NoteWordErrors
    {
        get
        {
            return noteWordErrors ??
                (noteWordErrors = new Repository<NoteWordError>(dbContext));
        }
    }

    public IRepository<UploadedModel> UploadedModels
    {
        get
        {
            return uploadedModels ??
                (uploadedModels = new Repository<UploadedModel>(dbContext));
        }
    }

    public IRepository<UserAction> UserActions
    {
        get
        {
            return userActions ??
                (userActions = new Repository<UserAction>(dbContext));
        }
    }

    public IRepository<UserLog> UserLogs
    {
        get
        {
            return userLogs ??
                (userLogs = new Repository<UserLog>(dbContext));
        }
    }

    public void Commit()
    {
        dbContext.SaveChanges();
    }
}