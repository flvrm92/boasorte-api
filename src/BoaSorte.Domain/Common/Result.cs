namespace BoaSorte.Domain.Common;

/// <summary>Outcome of an operation that can fail in expected ways (constitution §4).</summary>
public class Result
{
    protected Result(IReadOnlyList<Error> errors) => Errors = errors;

    public IReadOnlyList<Error> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public static Result<T> Success<T>(T value) => new(value, []);

    public static Result<T> Failure<T>(IReadOnlyList<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("A failure needs at least one error.", nameof(errors));
        }

        return new Result<T>(default, errors);
    }
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, IReadOnlyList<Error> errors)
        : base(errors) => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

#pragma warning disable CA2225 // Named alternates exist: Result.Success / Result.Failure.
    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure<T>([error]);
#pragma warning restore CA2225
}
