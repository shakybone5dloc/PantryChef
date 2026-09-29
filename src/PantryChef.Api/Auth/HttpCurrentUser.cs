using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using PantryChef.Application.Abstractions;

namespace PantryChef.Api.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId => accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}