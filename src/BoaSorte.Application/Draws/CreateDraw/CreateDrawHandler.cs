using BoaSorte.Application.Abstractions;
using BoaSorte.Domain.Common;
using BoaSorte.Domain.Draws;

namespace BoaSorte.Application.Draws.CreateDraw;

public sealed class CreateDrawHandler(IDrawRepository repository, IRandomSource random, TimeProvider time)
{
    public async Task<Result<DrawDetails>> HandleAsync(CreateDrawCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Validate every input first so the client gets all field errors in one response.
        var title = DrawTitle.Create(command.Title);
        var winnersCount = WinnersCount.Create(command.WinnersCount);
        var pool = BuildPool(command);

        Result[] inputs = [title, winnersCount, pool];
        var errors = inputs.SelectMany(r => r.Errors).ToList();
        if (errors.Count > 0)
        {
            return Result.Failure<DrawDetails>(errors);
        }

        var settings = new DrawSettings(winnersCount.Value, command.RemoveDuplicates, command.ShowParticipants);
        var draw = Draw.Conduct(title.Value, pool.Value, settings, random, time.GetUtcNow());
        if (draw.IsFailure)
        {
            return Result.Failure<DrawDetails>(draw.Errors);
        }

        await repository.AddAsync(draw.Value, cancellationToken).ConfigureAwait(false);

        return DrawDetails.From(draw.Value);
    }

    private static Result<IParticipantPool> BuildPool(CreateDrawCommand command) => command.Kind switch
    {
        DrawKind.List when command.Participants is null =>
            Error.Validation("participants.required", "Informe os participantes.", "participants"),
        DrawKind.List => Widen(ListPool.From(command.Participants, command.RemoveDuplicates)),

        DrawKind.Range when command.Range is null =>
            Error.Validation("range.required", "Informe o intervalo.", "range"),
        DrawKind.Range => Widen(NumberRange.Create(command.Range.Start, command.Range.End), r => new RangePool(r)),

        _ => throw new ArgumentOutOfRangeException(nameof(command), command.Kind, "Unknown draw kind."),
    };

    private static Result<IParticipantPool> Widen<T>(Result<T> result, Func<T, IParticipantPool> map) =>
        result.IsSuccess ? Result.Success(map(result.Value)) : Result.Failure<IParticipantPool>(result.Errors);

    private static Result<IParticipantPool> Widen(Result<ListPool> result) => Widen(result, pool => pool);
}
