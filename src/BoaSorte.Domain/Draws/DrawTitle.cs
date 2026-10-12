using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>Optional draw title. A blank input means "no title" (<c>null</c>).</summary>
public sealed record DrawTitle
{
    public const int MaxLength = 120;

    private DrawTitle(string value) => Value = value;

    public string Value { get; }

    public static Result<DrawTitle?> Create(string? raw)
    {
        var value = raw?.Trim();

        if (string.IsNullOrEmpty(value))
        {
            return Result.Success<DrawTitle?>(null);
        }

        if (value.Length > MaxLength)
        {
            return Error.Validation(
                "title.too_long",
                $"O título deve ter no máximo {MaxLength} caracteres.",
                "title");
        }

        return new DrawTitle(value);
    }

    public override string ToString() => Value;
}
