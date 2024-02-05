namespace SimpleErp.Validation;

public interface IApiKeyValidator
{
    Task<bool> IsValid(string apiKey);
}
