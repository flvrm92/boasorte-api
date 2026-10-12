using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>Unguessable identifier used in shared links: 12 base62 characters (~71 bits).</summary>
public sealed record PublicId
{
    public const int Length = 12;
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private PublicId(string value) => Value = value;

    public string Value { get; }

    public static PublicId New(IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var chars = new char[Length];
        for (var i = 0; i < Length; i++)
        {
            chars[i] = Alphabet[random.NextInt32(Alphabet.Length)];
        }

        return new PublicId(new string(chars));
    }

    public static Result<PublicId> Parse(string? raw) =>
        raw is { Length: Length } && raw.All(c => char.IsAsciiLetterOrDigit(c))
            ? new PublicId(raw)
            : Error.Validation("public_id.malformed", "Identificador de sorteio inválido.", "publicId");

    public override string ToString() => Value;
}
