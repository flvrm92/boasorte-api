using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>How many winners to draw (1 to <see cref="Max"/>). The comparison with eligible participants happens in <see cref="Draw"/>.</summary>
public sealed record WinnersCount
{
    public const int Max = 1000;

    private WinnersCount(int value) => Value = value;

    public int Value { get; }

    public static Result<WinnersCount> Create(int value) =>
        value is < 1 or > Max
            ? Error.Validation(
                "winners_count.out_of_range",
                $"O número de ganhadores deve estar entre 1 e {Max}.",
                "winnersCount")
            : new WinnersCount(value);
}
