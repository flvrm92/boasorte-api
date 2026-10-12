using BoaSorte.Domain.Draws;

namespace BoaSorte.Application.Draws.CreateDraw;

/// <summary>Official draw request (spec 001, FR-012). The server draws; the client never sends a result.</summary>
public sealed record CreateDrawCommand(
    string? Title,
    DrawKind Kind,
    IReadOnlyList<string?>? Participants,
    RangeInput? Range,
    bool RemoveDuplicates,
    int WinnersCount,
    bool ShowParticipants);

public sealed record RangeInput(int Start, int End);
