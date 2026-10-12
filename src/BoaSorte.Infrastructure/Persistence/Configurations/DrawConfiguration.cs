using BoaSorte.Domain.Draws;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoaSorte.Infrastructure.Persistence.Configurations;

/// <summary>Maps the <see cref="Draw"/> aggregate directly (specs/001-sorteio-lista/data-model.md).</summary>
internal sealed class DrawConfiguration : IEntityTypeConfiguration<Draw>
{
    public void Configure(EntityTypeBuilder<Draw> builder)
    {
        builder.ToTable("draws", table =>
        {
            table.HasCheckConstraint("ck_draws_kind", "kind IN ('list', 'range')");
            table.HasCheckConstraint("ck_draws_winners_count", "winners_count >= 1");
            table.HasCheckConstraint(
                "ck_draws_range",
                "(kind = 'list' AND range_start IS NULL AND range_end IS NULL) OR " +
                "(kind = 'range' AND range_start IS NOT NULL AND range_end >= range_start)");
        });

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.PublicId)
            .HasConversion(id => id.Value, value => PublicId.Parse(value).Value)
            .HasMaxLength(PublicId.Length);
        builder.HasIndex(d => d.PublicId).IsUnique();

        builder.Property(d => d.Title)
            .HasConversion(title => title!.Value, value => DrawTitle.Create(value).Value!)
            .HasMaxLength(DrawTitle.MaxLength);

        builder.Property(d => d.Kind)
            .HasConversion(kind => kind == DrawKind.List ? "list" : "range", value => value == "list" ? DrawKind.List : DrawKind.Range)
            .HasMaxLength(10);

        builder.ComplexProperty(d => d.Settings, settings =>
        {
            settings.Property(s => s.RemoveDuplicates).HasColumnName("remove_duplicates");
            settings.Property(s => s.ShowParticipants).HasColumnName("show_participants");
            settings.Property(s => s.WinnersCount)
                .HasConversion(count => count.Value, value => WinnersCount.Create(value).Value)
                .HasColumnName("winners_count");
        });

        builder.ComplexProperty(d => d.Range, range =>
        {
            range.Property(r => r.Start).HasColumnName("range_start");
            range.Property(r => r.End).HasColumnName("range_end");
        });

        builder.Property(d => d.Participants)
            .HasConversion(
                participants => participants.Select(p => p.Value).ToArray(),
                values => values.Select(v => Participant.Create(v).Value).ToList(),
                SequenceComparer<Participant>())
            .HasColumnType("text[]");

        // Position is implied by the array index (0 = first place).
        builder.Property(d => d.Winners)
            .HasConversion(
                winners => winners.OrderBy(w => w.Position).Select(w => w.Value).ToArray(),
                values => values.Select((value, index) => new Winner(index + 1, value)).ToList(),
                SequenceComparer<Winner>())
            .HasColumnType("text[]");

        builder.HasIndex(d => d.ExpiresAt);
    }

    private static ValueComparer<IReadOnlyList<T>> SequenceComparer<T>() => new(
        (left, right) => left!.SequenceEqual(right!),
        list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item!.GetHashCode())),
        list => list.ToList());
}
