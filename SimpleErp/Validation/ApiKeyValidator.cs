using SimpleErp.Services;

namespace SimpleErp.Validation;

public class ApiKeyValidator (IUserService userService) : IApiKeyValidator
{
    public async Task<bool> IsValid(string apiKey)
    {
        return await userService.ExistsByApiKey(apiKey);
    }
}
