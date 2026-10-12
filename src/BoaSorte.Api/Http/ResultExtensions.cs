using BoaSorte.Domain.Common;

namespace BoaSorte.Api.Http;

/// <summary>Maps expected failures to RFC 9457 problem responses.</summary>
internal static class ResultExtensions
{
    public static IResult ToProblem(this IReadOnlyList<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        var first = errors[0];

        return first.Type switch
        {
            ErrorType.NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: first.Message, extensions: Code(first)),
            ErrorType.Expired => TypedResults.Problem(statusCode: StatusCodes.Status410Gone, title: first.Message, extensions: Code(first)),
            _ => TypedResults.ValidationProblem(
                errors
                    .GroupBy(e => e.Field ?? e.Code)
                    .ToDictionary(group => group.Key, group => group.Select(e => e.Message).ToArray())),
        };
    }

    private static Dictionary<string, object?> Code(Error error) => new() { ["code"] = error.Code };
}
