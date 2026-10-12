namespace BoaSorte.Domain.Draws;

/// <summary>Configuration chosen by the organizer and recorded with the draw (constitution §1).</summary>
public sealed record DrawSettings(WinnersCount WinnersCount, bool RemoveDuplicates, bool ShowParticipants);
