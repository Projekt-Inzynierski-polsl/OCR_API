using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Repositories;


public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    void Commit();
}
public class UnitOfWork : IUnitOfWork
{

    private readonly SystemDbContext dbContext;
    private Repository<User> users;
    private Repository<Role> roles;

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

    public void Commit()
    {
        dbContext.SaveChanges();
    }
}