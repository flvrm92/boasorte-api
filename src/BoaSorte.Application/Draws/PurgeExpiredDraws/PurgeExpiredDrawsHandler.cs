using BoaSorte.Application.Abstractions;

namespace BoaSorte.Application.Draws.PurgeExpiredDraws;

/// <summary>
/// Removes draws expired for longer than <see cref="GracePeriod"/> (spec 001, FR-011). During the grace period the
/// link still answers "expired" (410) instead of "not found" (404).
/// </summary>
public sealed class PurgeExpiredDrawsHandler(IDrawRepository repository, TimeProvider time)
{
    public static readonly TimeSpan GracePeriod = TimeSpan.FromDays(30);

    public Task<int> HandleAsync(CancellationToken cancellationToken) =>
        repository.DeleteExpiredBeforeAsync(time.GetUtcNow() - GracePeriod, cancellationToken);
}
