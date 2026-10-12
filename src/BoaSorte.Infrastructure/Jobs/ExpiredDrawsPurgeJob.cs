using BoaSorte.Application.Draws.PurgeExpiredDraws;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BoaSorte.Infrastructure.Jobs;

public sealed class PurgeOptions
{
    /// <summary>How often expired draws are purged (spec 001, FR-011). Config: <c>Purge:Interval</c>.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromDays(1);
}

/// <summary>Runs <see cref="PurgeExpiredDrawsHandler"/> at startup and then every <see cref="PurgeOptions.Interval"/>.</summary>
internal sealed partial class ExpiredDrawsPurgeJob(
    IServiceScopeFactory scopes,
    IOptions<PurgeOptions> options,
    ILogger<ExpiredDrawsPurgeJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval);
        do
        {
            await PurgeOnceAsync(stoppingToken).ConfigureAwait(false);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "A failed run (e.g. database briefly unavailable) is logged and retried on the next tick.")]
    private async Task PurgeOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var deleted = await scope.ServiceProvider.GetRequiredService<PurgeExpiredDrawsHandler>()
                .HandleAsync(cancellationToken)
                .ConfigureAwait(false);

            if (deleted > 0)
            {
                LogPurged(deleted);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Purged {Count} expired draws.")]
    private partial void LogPurged(int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Purging expired draws failed; retrying on the next run.")]
    private partial void LogFailed(Exception exception);
}
