using BoaSorte.Application.Abstractions;
using BoaSorte.Domain.Draws;

namespace BoaSorte.Application.Tests.TestDoubles;

public sealed class InMemoryDrawRepository : IDrawRepository
{
    private readonly List<Draw> _draws = [];

    public IReadOnlyList<Draw> Draws => _draws;

    public Task AddAsync(Draw draw, CancellationToken cancellationToken)
    {
        _draws.Add(draw);
        return Task.CompletedTask;
    }

    public Task<Draw?> GetByPublicIdAsync(PublicId publicId, CancellationToken cancellationToken) =>
        Task.FromResult(_draws.SingleOrDefault(d => d.PublicId == publicId));

    public Task<int> DeleteExpiredBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken) =>
        Task.FromResult(_draws.RemoveAll(d => d.ExpiresAt < cutoff));
}
