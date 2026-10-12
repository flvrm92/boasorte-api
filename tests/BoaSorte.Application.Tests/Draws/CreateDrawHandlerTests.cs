using BoaSorte.Application.Draws;
using BoaSorte.Domain.Draws;
using static BoaSorte.Application.Tests.Draws.DrawsTestContext;

namespace BoaSorte.Application.Tests.Draws;

public class CreateDrawHandlerTests
{
    private readonly DrawsTestContext _context = new();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Creates_and_persists_a_list_draw()
    {
        var command = ListCommand(["Ana", "ana", "Bruno", "", "Carla"], winnersCount: 2, removeDuplicates: true, title: " Rifa ");

        var details = (await _context.Create.HandleAsync(command, Ct)).Value;

        var stored = Assert.Single(_context.Repository.Draws);
        Assert.Equal(stored.PublicId.Value, details.PublicId);
        Assert.Equal("Rifa", details.Title);
        Assert.Equal(DrawKind.List, details.Kind);
        Assert.Equal(5, details.SubmittedCount);
        Assert.Equal(3, details.EligibleCount);
        Assert.Equal(["Ana", "Bruno", "Carla"], details.Participants);
        Assert.Equal([1, 2], details.Winners.Select(w => w.Position));
        Assert.Equal(new DrawSettingsDetails(true, 2, true, null), details.Settings);
    }

    [Fact]
    public async Task Uses_the_current_time_for_creation_and_expiry()
    {
        var details = (await _context.Create.HandleAsync(ListCommand(), Ct)).Value;

        Assert.Equal(Start, details.CreatedAt);
        Assert.Equal(Start.AddMonths(12), details.ExpiresAt);
    }

    [Fact]
    public async Task Creates_a_range_draw_without_exposing_participants()
    {
        var details = (await _context.Create.HandleAsync(RangeCommand(1, 500, winnersCount: 2), Ct)).Value;

        Assert.Equal(DrawKind.Range, details.Kind);
        Assert.Null(details.Participants);
        Assert.Equal(500, details.EligibleCount);
        Assert.Equal(new DrawSettingsDetails(false, 2, true, new RangeDetails(1, 500)), details.Settings);
    }

    [Fact]
    public async Task Hides_participants_when_the_organizer_chose_so()
    {
        var details = (await _context.Create.HandleAsync(ListCommand(showParticipants: false), Ct)).Value;

        Assert.Null(details.Participants);
        Assert.NotEmpty(Assert.Single(_context.Repository.Draws).Participants); // kept for auditing
    }

    [Fact]
    public async Task Reports_every_invalid_field_at_once_and_persists_nothing()
    {
        var command = ListCommand(winnersCount: 0, title: new string('x', DrawTitle.MaxLength + 1));

        var result = await _context.Create.HandleAsync(command, Ct);

        Assert.Equal(["title", "winnersCount"], result.Errors.Select(e => e.Field).Order());
        Assert.Empty(_context.Repository.Draws);
    }

    [Fact]
    public async Task List_draw_requires_participants()
    {
        var command = ListCommand() with { Participants = null };

        var error = Assert.Single((await _context.Create.HandleAsync(command, Ct)).Errors);

        Assert.Equal(("participants.required", "participants"), (error.Code, error.Field));
    }

    [Fact]
    public async Task Range_draw_requires_a_range()
    {
        var command = RangeCommand() with { Range = null };

        var error = Assert.Single((await _context.Create.HandleAsync(command, Ct)).Errors);

        Assert.Equal(("range.required", "range"), (error.Code, error.Field));
    }

    [Fact]
    public async Task Invalid_range_is_rejected()
    {
        var error = Assert.Single((await _context.Create.HandleAsync(RangeCommand(10, 1), Ct)).Errors);

        Assert.Equal("range", error.Field);
    }

    [Fact]
    public async Task Entries_that_are_too_long_point_at_their_index()
    {
        var command = ListCommand(["Ana", new string('x', Participant.MaxLength + 1)]);

        var error = Assert.Single((await _context.Create.HandleAsync(command, Ct)).Errors);

        Assert.Equal("participants[1]", error.Field);
    }

    [Fact]
    public async Task More_winners_than_eligible_participants_is_rejected_and_nothing_is_persisted()
    {
        var command = ListCommand(["Ana", "ana"], winnersCount: 2, removeDuplicates: true);

        var error = Assert.Single((await _context.Create.HandleAsync(command, Ct)).Errors);

        Assert.Equal("draw.not_enough_participants", error.Code);
        Assert.Empty(_context.Repository.Draws);
    }
}
