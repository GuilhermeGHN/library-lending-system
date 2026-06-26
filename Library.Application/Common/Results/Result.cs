namespace Library.Application.Common.Results;

public sealed class Result<T>
{
    #region Constructors

    private Result(bool success, T? value, Error? error)
    {
        Success = success;
        Value = value;
        Error = error;
    }

    #endregion

    #region Properties

    public bool Success { get; }

    public T? Value { get; }

    public Error? Error { get; }

    #endregion

    #region Public Methods

    public static Result<T> Ok(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Fail(Error error)
    {
        return new Result<T>(false, default, error);
    }

    #endregion
}
