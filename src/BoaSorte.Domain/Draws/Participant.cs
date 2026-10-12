using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>A single entry of a list draw: trimmed, non-empty, at most <see cref="MaxLength"/> characters.</summary>
public sealed record Participant
{
    public const int MaxLength = 200;

    private Participant(string value) => Value = value;

    public string Value { get; }

    /// <summary>Key used to detect duplicates: case-insensitive, accent-sensitive (spec 001, FR-004).</summary>
    public string DedupKey => Value.ToLowerInvariant();

    public static Result<Participant> Create(string? raw)
    {
        var value = raw?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            return Error.Validation("participant.empty", "O participante não pode ser vazio.");
        }

        if (value.Length > MaxLength)
        {
            return Error.Validation(
                "participant.too_long",
                $"O participante deve ter no máximo {MaxLength} caracteres.");
        }

        return new Participant(value);
    }

    public override string ToString() => Value;
}
