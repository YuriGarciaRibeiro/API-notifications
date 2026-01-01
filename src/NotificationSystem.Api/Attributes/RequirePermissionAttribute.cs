using Microsoft.AspNetCore.Authorization;

namespace NotificationSystem.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"RequirePermission:{permission}";
    }
}
