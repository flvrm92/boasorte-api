using BoaSorte.Application.Abstractions;
using BoaSorte.Domain.Draws;
using Microsoft.EntityFrameworkCore;

namespace BoaSorte.Infrastructure.Persistence;

internal sealed class DrawRepository(AppDbContext db) : IDrawRepository
{
    public async Task AddAsync(Draw draw, CancellationToken cancellationToken)
    {
        db.Draws.Add(draw);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<Draw?> GetByPublicIdAsync(PublicId publicId, CancellationToken cancellationToken) =>
        db.Draws.AsNoTracking().SingleOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);

    public Task<int> DeleteExpiredBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken) =>
        db.Draws.Where(d => d.ExpiresAt < cutoff).ExecuteDeleteAsync(cancellationToken);
}
