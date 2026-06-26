using Library.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Common;

public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    #region Public Methods

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            DomainValidationException => (StatusCodes.Status400BadRequest, "Domain validation failed"),
            BookUnavailableException or LoanAlreadyReturnedException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status >= 500)
            logger.LogError(exception, "Unhandled request failure");
        else
            logger.LogWarning(exception, "Request failed with status {StatusCode}", status);

        context.Response.StatusCode = status;

        var details = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        details.Extensions["correlationId"] = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = details,
            Exception = exception
        });
    }

    #endregion
}
