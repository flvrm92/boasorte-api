using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>
/// An official, immutable draw (spec 001, FR-009/FR-012): the eligible participants, the settings, the
/// winners and when it happened. Only <see cref="Conduct"/> creates one; nothing changes it afterwards.
/// </summary>
public sealed class Draw
{
    public const int RetentionMonths = 12;

    // Used by EF Core to materialize persisted draws.
    private Draw()
    {
    }

    public Guid Id { get; private set; }

    public PublicId PublicId { get; private set; } = null!;

    public DrawTitle? Title { get; private set; }

    public DrawKind Kind { get; private set; }

    public DrawSettings Settings { get; private set; } = null!;

    /// <summary>Only for <see cref="DrawKind.Range"/>.</summary>
    public NumberRange? Range { get; private set; }

    public long SubmittedCount { get; private set; }

    public long EligibleCount { get; private set; }

    /// <summary>Eligible participants in their original order. Empty for <see cref="DrawKind.Range"/>.</summary>
    public IReadOnlyList<Participant> Participants { get; private set; } = [];

    /// <summary>Ordered: index 0 is first place.</summary>
    public IReadOnlyList<Winner> Winners { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public static Result<Draw> Conduct(
        DrawTitle? title,
        IParticipantPool pool,
        DrawSettings settings,
        IRandomSource random,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(random);

        if (pool.Count == 0)
        {
            return DrawErrors.NoEligibleParticipants();
        }

        if (settings.WinnersCount.Value > pool.Count)
        {
            return DrawErrors.NotEnoughParticipants(pool.Count);
        }

        var winners = WinnerSampler.Sample(pool.Count, settings.WinnersCount.Value, random)
            .Select((index, i) => new Winner(i + 1, pool.ValueAt(index)))
            .ToArray();

        return new Draw
        {
            Id = Guid.CreateVersion7(now),
            PublicId = PublicId.New(random),
            Title = title,
            Kind = pool.Kind,
            Settings = pool is RangePool ? settings with { RemoveDuplicates = false } : settings,
            Range = (pool as RangePool)?.Range,
            SubmittedCount = pool.SubmittedCount,
            EligibleCount = pool.Count,
            Participants = (pool as ListPool)?.Participants ?? [],
            Winners = winners,
            CreatedAt = now,
            ExpiresAt = now.AddMonths(RetentionMonths),
        };
    }

    public bool IsExpiredAt(DateTimeOffset now) => now >= ExpiresAt;
}
