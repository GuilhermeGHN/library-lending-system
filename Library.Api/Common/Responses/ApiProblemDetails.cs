using Library.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Common.Responses;

/// <summary>
/// RFC 7807 error response used by the API.
/// </summary>
/// <remarks>
/// The standard fields come from <see cref="ProblemDetails"/>. Additional fields are emitted through ProblemDetails extensions.
/// </remarks>
public sealed class ApiProblemDetails : ProblemDetails
{
    #region Properties

    /// <summary>
    /// Stable application error code, such as validation, book.not_found or resource.concurrency.conflict.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Field-level validation details when the error represents invalid input.
    /// </summary>
    public IReadOnlyCollection<ErrorDetail>? Errors { get; init; }

    /// <summary>
    /// Optional structured context for the error.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Payload { get; init; }

    /// <summary>
    /// Correlation id returned for unexpected or exception-based failures.
    /// </summary>
    public string? CorrelationId { get; init; }

    #endregion
}
