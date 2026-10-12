using BoaSorte.Domain.Draws;

namespace BoaSorte.Application.Draws;

/// <summary>Public view of a draw (spec 001, US5.2). Privacy rules are applied here, not in the HTTP layer.</summary>
public sealed record DrawDetails(
    string PublicId,
    string? Title,
    DrawKind Kind,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DrawSettingsDetails Settings,
    long SubmittedCount,
    long EligibleCount,
    IReadOnlyList<WinnerDetails> Winners,
    IReadOnlyList<string>? Participants)
{
    public static DrawDetails From(Draw draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        var showParticipants = draw.Kind == DrawKind.List && draw.Settings.ShowParticipants;

        return new DrawDetails(
            draw.PublicId.Value,
            draw.Title?.Value,
            draw.Kind,
            draw.CreatedAt,
            draw.ExpiresAt,
            new DrawSettingsDetails(
                draw.Settings.RemoveDuplicates,
                draw.Settings.WinnersCount.Value,
                draw.Settings.ShowParticipants,
                draw.Range is { } range ? new RangeDetails(range.Start, range.End) : null),
            draw.SubmittedCount,
            draw.EligibleCount,
            [.. draw.Winners.Select(w => new WinnerDetails(w.Position, w.Value))],
            showParticipants ? [.. draw.Participants.Select(p => p.Value)] : null);
    }
}

public sealed record DrawSettingsDetails(bool RemoveDuplicates, int WinnersCount, bool ShowParticipants, RangeDetails? Range);

public sealed record RangeDetails(int Start, int End);

public sealed record WinnerDetails(int Position, string Value);
