using Microsoft.AspNetCore.Mvc;

namespace SimpleErp.Authorization;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}