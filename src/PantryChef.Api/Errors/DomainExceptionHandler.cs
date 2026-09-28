using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PantryChef.Domain;

namespace PantryChef.Api.Errors;

public sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException domain) return false;

        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Business rule violated",
                Detail = domain.Message
            }
        });
    }
}