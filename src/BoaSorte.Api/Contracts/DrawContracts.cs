using BoaSorte.Application.Draws;
using BoaSorte.Application.Draws.CreateDraw;
using BoaSorte.Domain.Common;
using BoaSorte.Domain.Draws;

namespace BoaSorte.Api.Contracts;

// HTTP contract v1 — specs/001-sorteio-lista/contracts/api.md. Mapped explicitly to and from the Application layer.

public sealed record CreateDrawRequest(
    string? Title,
    string? Kind,
    IReadOnlyList<string?>? Participants,
    RangeRequest? Range,
    bool RemoveDuplicates,
    int? WinnersCount,
    bool? ShowParticipants)
{
    /// <summary>Checks the request shape; business rules are validated by the use case.</summary>
    public Result<CreateDrawCommand> ToCommand()
    {
        var errors = new List<Error>();

        DrawKind? kind = Kind?.ToLowerInvariant() switch
        {
            "list" => DrawKind.List,
            "range" => DrawKind.Range,
            _ => null,
        };
        if (kind is null)
        {
            errors.Add(Error.Validation("kind.invalid", "Informe o tipo do sorteio: \"list\" ou \"range\".", "kind"));
        }

        if (WinnersCount is null)
        {
            errors.Add(Error.Validation("winners_count.required", "Informe o número de ganhadores.", "winnersCount"));
        }

        if (errors.Count > 0)
        {
            return Result.Failure<CreateDrawCommand>(errors);
        }

        return new CreateDrawCommand(
            Title,
            kind!.Value,
            Participants,
            Range is null ? null : new RangeInput(Range.Start, Range.End),
            RemoveDuplicates,
            WinnersCount!.Value,
            ShowParticipants ?? true);
    }
}

public sealed record RangeRequest(int Start, int End);

public sealed record DrawResponse(
    string PublicId,
    string? Title,
    string Kind,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DrawSettingsResponse Settings,
    long SubmittedCount,
    long EligibleCount,
    IReadOnlyList<WinnerResponse> Winners,
    IReadOnlyList<string>? Participants)
{
    public static DrawResponse From(DrawDetails draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        return new DrawResponse(
            draw.PublicId,
            draw.Title,
            draw.Kind == DrawKind.List ? "list" : "range",
            draw.CreatedAt,
            draw.ExpiresAt,
            new DrawSettingsResponse(
                draw.Settings.RemoveDuplicates,
                draw.Settings.WinnersCount,
                draw.Settings.ShowParticipants,
                draw.Settings.Range is { } range ? new RangeResponse(range.Start, range.End) : null),
            draw.SubmittedCount,
            draw.EligibleCount,
            [.. draw.Winners.Select(w => new WinnerResponse(w.Position, w.Value))],
            draw.Participants);
    }
}

public sealed record DrawSettingsResponse(bool RemoveDuplicates, int WinnersCount, bool ShowParticipants, RangeResponse? Range);

public sealed record RangeResponse(int Start, int End);

public sealed record WinnerResponse(int Position, string Value);
