using BoaSorte.Application.Abstractions;
using BoaSorte.Domain.Common;
using BoaSorte.Domain.Draws;

namespace BoaSorte.Application.Draws.GetDraw;

public sealed record GetDrawQuery(string PublicId);

public sealed class GetDrawHandler(IDrawRepository repository, TimeProvider time)
{
    public async Task<Result<DrawDetails>> HandleAsync(GetDrawQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // A malformed id cannot exist, so it is reported exactly like an unknown one.
        var publicId = PublicId.Parse(query.PublicId);
        if (publicId.IsFailure)
        {
            return DrawErrors.NotFound();
        }

        var draw = await repository.GetByPublicIdAsync(publicId.Value, cancellationToken).ConfigureAwait(false);
        if (draw is null)
        {
            return DrawErrors.NotFound();
        }

        return draw.IsExpiredAt(time.GetUtcNow())
            ? DrawErrors.Expired()
            : DrawDetails.From(draw);
    }
}
