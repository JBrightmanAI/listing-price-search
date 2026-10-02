namespace ListingSearch.Application.Common;

/// <summary>
/// Outcome of a use case. Invalid input is an expected outcome, so it is returned as a value
/// instead of thrown; exceptions are reserved for genuine faults.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly ValidationErrors? _errors;

    internal Result(T value)
    {
        _value = value;
    }

    internal Result(ValidationErrors errors)
    {
        _errors = errors;
    }

    public bool IsSuccess => _errors is null;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    public ValidationErrors Errors => _errors
        ?? throw new InvalidOperationException("Cannot read the errors of a successful result.");
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Invalid<T>(ValidationErrors errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return new Result<T>(errors);
    }
}
