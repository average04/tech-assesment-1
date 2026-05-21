namespace CustomerOnboarding.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public IReadOnlyList<string> Errors { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, IReadOnlyList<string> errors, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, Array.Empty<string>(), ErrorType.None);

    public static Result Failure(ErrorType type, params string[] errors) =>
        new(false, errors, type);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, IReadOnlyList<string> errors, ErrorType errorType)
        : base(isSuccess, errors, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) =>
        new(true, value, Array.Empty<string>(), ErrorType.None);

    public static new Result<T> Failure(ErrorType type, params string[] errors) =>
        new(false, default, errors, type);
}
