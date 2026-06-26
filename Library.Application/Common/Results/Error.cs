namespace Library.Application.Common.Results;

public sealed record Error(
    string Code,
    string Message,
    IReadOnlyCollection<ErrorDetail>? Details = null,
    IReadOnlyDictionary<string, object?>? Payload = null)
{
    #region Public Methods

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors)
    {
        var details = errors
            .SelectMany(error => error.Value.Select(message => new ErrorDetail(error.Key, message)))
            .ToArray();

        return new Error("validation", "Validation failed.", details);
    }

    public static Error Invalid(string code, string message)
    {
        return new Error(EnsureSuffix(code, ".invalid"), message);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(EnsureSuffix(code, ".not_found"), message);
    }

    public static Error Conflict(string code, string message, IReadOnlyDictionary<string, object?>? payload = null)
    {
        return new Error(EnsureSuffix(code, ".conflict"), message, Payload: payload);
    }

    #endregion

    #region Private Methods

    private static string EnsureSuffix(string code, string suffix)
    {
        return code.EndsWith(suffix, StringComparison.Ordinal)
            ? code
            : $"{code}{suffix}";
    }

    #endregion
}
