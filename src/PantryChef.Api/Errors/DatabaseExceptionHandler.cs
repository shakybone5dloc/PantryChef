using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PantryChef.Api.Errors;

public sealed class DatabaseExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<DatabaseExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg })
            return false;   // not ours, so the next handler (or the default 500) takes it

        logger.LogWarning("Unique constraint violated: {Constraint}", pg.ConstraintName);

        context.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "Another request created this at the same moment. Please retry."
            }
        });
    }
}