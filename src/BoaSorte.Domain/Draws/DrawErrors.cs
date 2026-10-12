using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

public static class DrawErrors
{
    public static Error NoEligibleParticipants() =>
        Error.Validation("draw.no_eligible_participants", "Não há participantes elegíveis.", "participants");

    public static Error NotEnoughParticipants(long eligible) =>
        Error.Validation(
            "draw.not_enough_participants",
            eligible == 1 ? "Há apenas 1 participante elegível." : $"Há apenas {eligible} participantes elegíveis.",
            "winnersCount");

    public static Error NotFound() => Error.NotFound("draw.not_found", "Sorteio não encontrado.");

    public static Error Expired() => Error.Expired("draw.expired", "Este sorteio expirou.");
}
