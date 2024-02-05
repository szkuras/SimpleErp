using Microsoft.EntityFrameworkCore;
using SimpleErp.Models;

namespace SimpleErp.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;

        var usersInit = new List<User>
        {
             new User
                {
                    Id = new Guid(),
                    ApiKey = "filub2li3br2fgukqvk2j1v3ruyg"
                },
            new User
                {
                    Id = new Guid(),
                    ApiKey = "hjbkhv32k4jkhvkj435hjv"
                },
            new User
                {
                    Id = new Guid(),
                    ApiKey = "asdas7dt76rsdicyvu"
                }
        };

        _dbContext.Users.AddRange(usersInit);
        _dbContext.SaveChanges();
    }

    public async Task<bool> ExistsByApiKey(string apiKey)
    {
        return await _dbContext.Users.AnyAsync(u => u.ApiKey == apiKey);
    }

    public async Task<User?> GetByApiKey(string apiKey)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    }
}