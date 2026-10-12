using BoaSorte.Domain.Draws;

namespace BoaSorte.Application.Abstractions;

/// <summary>Persistence port for <see cref="Draw"/>. Draws are immutable: there is no update.</summary>
public interface IDrawRepository
{
    Task AddAsync(Draw draw, CancellationToken cancellationToken);

    Task<Draw?> GetByPublicIdAsync(PublicId publicId, CancellationToken cancellationToken);

    /// <summary>Deletes draws whose <see cref="Draw.ExpiresAt"/> is before <paramref name="cutoff"/>; returns how many.</summary>
    Task<int> DeleteExpiredBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);
}
