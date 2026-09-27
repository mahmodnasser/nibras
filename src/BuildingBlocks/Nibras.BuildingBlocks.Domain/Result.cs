namespace Nibras.BuildingBlocks.Domain;

/// <summary>The outcome of an operation that can fail for an expected reason. Exceptions are for the unexpected.</summary>
public class Result
{
    private protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(null);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(error);
    }

    public static Result<T> Success<T>(T value) => new(value, null);

    public static Result<T> Failure<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(default, error);
    }
}

/// <summary>An outcome that carries a value when it succeeds. Created through <see cref="Result.Success{T}"/> and <see cref="Result.Failure{T}"/>.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, Error? error)
        : base(error) => _value = value;

    /// <summary>The value; reading it from a failure is a programming error.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"A failed result has no value; the error is {Error!.Code}.");
}
