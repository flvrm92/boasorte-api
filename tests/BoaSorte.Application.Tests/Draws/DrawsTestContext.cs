using BoaSorte.Application.Draws.CreateDraw;
using BoaSorte.Application.Draws.GetDraw;
using BoaSorte.Application.Draws.PurgeExpiredDraws;
using BoaSorte.Application.Tests.TestDoubles;
using BoaSorte.Domain.Draws;
using Microsoft.Extensions.Time.Testing;

namespace BoaSorte.Application.Tests.Draws;

/// <summary>Wires the draw use cases to in-memory fakes, the way DI wires them to real adapters.</summary>
public sealed class DrawsTestContext
{
    public static readonly DateTimeOffset Start = new(2026, 10, 12, 18, 0, 0, TimeSpan.Zero);

    public DrawsTestContext()
    {
        Create = new CreateDrawHandler(Repository, Random, Time);
        Get = new GetDrawHandler(Repository, Time);
        Purge = new PurgeExpiredDrawsHandler(Repository, Time);
    }

    public InMemoryDrawRepository Repository { get; } = new();

    public SeededRandomSource Random { get; } = new(2026);

    public FakeTimeProvider Time { get; } = new(Start);

    public CreateDrawHandler Create { get; }

    public GetDrawHandler Get { get; }

    public PurgeExpiredDrawsHandler Purge { get; }

    public static CreateDrawCommand ListCommand(
        string[]? participants = null,
        int winnersCount = 1,
        bool removeDuplicates = false,
        bool showParticipants = true,
        string? title = null) =>
        new(title, DrawKind.List, participants ?? ["Ana", "Bruno", "Carla"], null, removeDuplicates, winnersCount, showParticipants);

    public static CreateDrawCommand RangeCommand(int start = 1, int end = 500, int winnersCount = 1) =>
        new(null, DrawKind.Range, null, new RangeInput(start, end), RemoveDuplicates: true, winnersCount, ShowParticipants: true);
}
