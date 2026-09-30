using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using PantryChef.Application.Abstractions;

namespace PantryChef.Api.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private bool _resolved;
    private string? _userId;

    public string? UserId
    {
        get
        {
            if (!_resolved)
            {
                _userId = accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                _resolved = true;
            }
            return _userId;
        }
    }
}