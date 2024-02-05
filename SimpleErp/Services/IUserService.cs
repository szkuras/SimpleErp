using SimpleErp.Models;

namespace SimpleErp.Services;

public interface IUserService
{
    Task<bool> ExistsByApiKey(string apiKey);
    Task<User?>  GetByApiKey(string apiKey);
}
