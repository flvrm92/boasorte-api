using System.Threading.RateLimiting;

namespace BoaSorte.Api.Http;

/// <summary>Limits official draws per client IP (spec 001, FR-013). The IP comes from X-Forwarded-For (Traefik).</summary>
internal static class RateLimiting
{
    public const string CreateDrawPolicy = "create-draw";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:CreateDraw:PermitLimit", 10);

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
                context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new()
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails =
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Muitos sorteios em pouco tempo. Aguarde um minuto e tente de novo.",
                    },
                });

            options.AddPolicy(CreateDrawPolicy, http => RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });
    }
}
