using Library.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Common.Results;

public static class ResultExtensions
{
    #region Public Methods

    public static IActionResult ToActionResult(this Result result)
    {
        return result.Success
            ? new NoContentResult()
            : ToErrorActionResult(result.Error!);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.Success
            ? new OkObjectResult(result.Value)
            : ToErrorActionResult(result.Error!);
    }

    public static IActionResult ToCreatedAtActionResult<T>(
        this Result<T> result,
        string actionName,
        Func<T, object> routeValuesFactory,
        Func<T, object> responseFactory)
    {
        return result.Success
            ? new CreatedAtActionResult(actionName, null, routeValuesFactory(result.Value!), responseFactory(result.Value!))
            : ToErrorActionResult(result.Error!);
    }

    public static IActionResult ToCreatedAtActionResult<T>(
        this Result<T> result,
        string actionName,
        string controllerName,
        Func<T, object> routeValuesFactory,
        Func<T, object> responseFactory)
    {
        return result.Success
            ? new CreatedAtActionResult(actionName, controllerName, routeValuesFactory(result.Value!), responseFactory(result.Value!))
            : ToErrorActionResult(result.Error!);
    }

    #endregion

    #region Private Methods

    private static IActionResult ToErrorActionResult(Error error)
    {
        var status = ResolveStatusCode(error.Code);
        var details = CreateProblemDetails(error, status);

        return new ObjectResult(details)
        {
            StatusCode = status
        };
    }

    private static int ResolveStatusCode(string code)
    {
        return code switch
        {
            "validation" => StatusCodes.Status400BadRequest,
            var value when value.Contains(".validation.", StringComparison.Ordinal) => StatusCodes.Status400BadRequest,
            var value when value.EndsWith(".invalid", StringComparison.Ordinal) => StatusCodes.Status400BadRequest,
            var value when value.EndsWith(".not_found", StringComparison.Ordinal) => StatusCodes.Status404NotFound,
            var value when value.EndsWith(".conflict", StringComparison.Ordinal) => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static ProblemDetails CreateProblemDetails(Error error, int status)
    {
        var details = new ProblemDetails
        {
            Status = status,
            Title = ResolveTitle(status),
            Detail = error.Message
        };

        details.Extensions["code"] = error.Code;

        if (error.Details is not null)
            details.Extensions["errors"] = error.Details;

        if (error.Payload is not null)
            details.Extensions["payload"] = error.Payload;

        return details;
    }

    private static string ResolveTitle(int status)
    {
        return status switch
        {
            StatusCodes.Status400BadRequest => "Validation failed",
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Unexpected error"
        };
    }

    #endregion
}
