using Microsoft.AspNetCore.Authorization;

namespace ChatBotWithSignalR.Permission
{
    public record PermissionRequirement(string Permission) : IAuthorizationRequirement;
}
