namespace Library.Application.Common.Results;

/// <summary>
/// Detailed error item used in validation ProblemDetails responses.
/// </summary>
/// <param name="Code">Field name or domain-specific detail code.</param>
/// <param name="Message">Human-readable validation message.</param>
public sealed record ErrorDetail(string Code, string Message);
