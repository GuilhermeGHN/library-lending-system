namespace Library.Application.Common.Results;

public sealed class Result
{
    #region Constructors

    private Result(bool success, Error? error)
    {
        Success = success;
        Error = error;
    }

    #endregion

    #region Properties

    public bool Success { get; }

    public Error? Error { get; }

    #endregion

    #region Public Methods

    public static Result Ok()
    {
        return new Result(true, null);
    }

    public static Result Fail(Error error)
    {
        return new Result(false, error);
    }

    #endregion
}
