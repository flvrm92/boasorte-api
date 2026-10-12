using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Tests.Common;

public class ResultTests
{
    private static readonly Error SomeError = Error.Validation("some.code", "Mensagem.", "field");

    [Fact]
    public void Success_exposes_value_and_no_errors()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_exposes_errors_and_hides_value()
    {
        var result = Result.Failure<int>([SomeError]);

        Assert.True(result.IsFailure);
        Assert.Equal([SomeError], result.Errors);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_without_errors_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure<int>([]));
    }

    [Fact]
    public void Error_converts_implicitly_to_failed_result()
    {
        Result<string> result = SomeError;

        Assert.Equal(SomeError, Assert.Single(result.Errors));
    }

    [Fact]
    public void Error_factories_set_the_type()
    {
        Assert.Equal(ErrorType.Validation, Error.Validation("a", "b").Type);
        Assert.Equal(ErrorType.NotFound, Error.NotFound("a", "b").Type);
        Assert.Equal(ErrorType.Expired, Error.Expired("a", "b").Type);
    }
}
