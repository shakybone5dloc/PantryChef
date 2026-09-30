using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using PantryChef.Infrastructure.Identity;
using PantryChef.Contracts;

namespace PantryChef.Api.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapGet("/me", Me).RequireAuthorization();

        return app;
    }

    private static async Task<Results<Created, ValidationProblem>> Register(
        RegisterRequest request, UserManager<AppUser> users)
    {
        var user = new AppUser { UserName = request.Email, Email = request.Email };
        var result = await users.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return TypedResults.ValidationProblem(result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));

        return TypedResults.Created("/auth/me");
    }

    private static async Task<Results<Ok<AccessToken>, UnauthorizedHttpResult>> Login(
        LoginRequest request, UserManager<AppUser> users, SignInManager<AppUser> signIn, TokenService tokens)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null) return TypedResults.Unauthorized();

        // Checks the hash AND counts failures toward lockout
        var check = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!check.Succeeded) return TypedResults.Unauthorized();

        return TypedResults.Ok(tokens.CreateToken(user));
    }

    private static Ok<MeResponse> Me(ClaimsPrincipal user) =>
        TypedResults.Ok(new MeResponse(
            user.FindFirstValue(JwtRegisteredClaimNames.Sub)!,
            user.FindFirstValue(JwtRegisteredClaimNames.Email)!));
}