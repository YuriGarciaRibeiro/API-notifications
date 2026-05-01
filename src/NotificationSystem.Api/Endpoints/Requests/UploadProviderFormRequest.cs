using System.Reflection;

namespace NotificationSystem.Api.Endpoints.Requests;

public sealed record UploadProviderFormRequest(
    IFormFile? File,
    string? ChannelType,
    string? Provider,
    string? ProjectId,
    string? IsActive,
    string? IsPrimary)
{
    public static async ValueTask<UploadProviderFormRequest?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        if (!context.Request.HasFormContentType)
        {
            return new UploadProviderFormRequest(null, null, null, null, null, null);
        }

        var form = await context.Request.ReadFormAsync(context.RequestAborted);

        return new UploadProviderFormRequest(
            form.Files["file"],
            form["channelType"].ToString(),
            form["provider"].ToString(),
            form["projectId"].ToString(),
            form["isActive"].ToString(),
            form["isPrimary"].ToString());
    }
}
