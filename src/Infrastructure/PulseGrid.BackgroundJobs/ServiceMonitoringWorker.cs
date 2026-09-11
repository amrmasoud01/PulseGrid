namespace PulseGrid.BackgroundJobs;

using System.Diagnostics;
using System.Net.Http;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Application.Incidents.Commands.CreateIncident;
using PulseGrid.Application.SystemStatus.Queries.GetSystemStatus;
using PulseGrid.Domain.Entities;
using PulseGrid.Domain.Enums;

public class ServiceMonitoringWorker(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    ILogger<ServiceMonitoringWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ExecuteHealthChecksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred during health check execution cycle.");
            }
        }
    }

    private async Task ExecuteHealthChecksAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var notifier = scope.ServiceProvider.GetService<IStatusNotifier>();

        var services = await dbContext.MonitoredServices
            .Where(s => s.IsActive)
            .ToListAsync(ct);
        if (services.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var dueServices = services
            .Where(s => s.LastCheckedAtUtc is null || s.LastCheckedAtUtc.Value.AddSeconds(Math.Max(1, s.IntervalSeconds)) <= now)
            .ToList();

        if (dueServices.Count == 0)
        {
            return;
        }

        var client = httpClientFactory.CreateClient("HealthCheckClient");

        var checkTasks = dueServices.Select(service => PingServiceAsync(service, client, ct));
        var checkResults = await Task.WhenAll(checkTasks);

        var hasStatusChange = false;

        foreach (var result in checkResults)
        {
            var service = result.Service;
            var previousStatus = service.CurrentStatus;

            service.CurrentStatus = result.EvaluatedStatus;
            service.LastCheckedAtUtc = result.CheckedAtUtc;

            var log = new HealthCheckLog
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                StatusCode = result.StatusCode,
                LatencyMs = result.LatencyMs,
                IsSuccess = result.IsSuccess,
                CheckedAtUtc = result.CheckedAtUtc
            };

            dbContext.HealthCheckLogs.Add(log);

            if (result.EvaluatedStatus != previousStatus)
            {
                hasStatusChange = true;
            }

            if (result.EvaluatedStatus == ServiceStatus.Down)
            {
                var hasActiveIncident = await dbContext.Incidents
                    .AnyAsync(i => i.ServiceId == service.Id && i.Status != IncidentStatus.Resolved, ct);

                if (!hasActiveIncident)
                {
                    await mediator.Send(new CreateIncidentCommand(
                        service.Id,
                        $"Outage detected on '{service.Name}'",
                        IncidentSeverity.Critical,
                        $"Automated health check failed at {result.CheckedAtUtc:u}. Status code: {result.StatusCode?.ToString() ?? "N/A"}, Latency: {result.LatencyMs}ms."),
                        ct);

                    hasStatusChange = true;
                }
            }
            else if (result.EvaluatedStatus == ServiceStatus.Operational && previousStatus == ServiceStatus.Down)
            {
                var activeIncidents = await dbContext.Incidents
                    .Include(i => i.Updates)
                    .Where(i => i.ServiceId == service.Id && i.Status != IncidentStatus.Resolved)
                    .ToListAsync(ct);

                foreach (var incident in activeIncidents)
                {
                    incident.Status = IncidentStatus.Resolved;
                    incident.ResolvedAtUtc = DateTime.UtcNow;
                    incident.Updates.Add(new IncidentUpdate
                    {
                        Id = Guid.NewGuid(),
                        IncidentId = incident.Id,
                        Message = $"Automated recovery detected. Service '{service.Name}' is operational with {result.LatencyMs}ms latency.",
                        StatusSnapshot = IncidentStatus.Resolved,
                        TimestampUtc = DateTime.UtcNow,
                        AuthorUserId = null
                    });
                }

                if (activeIncidents.Count > 0)
                {
                    hasStatusChange = true;
                }
            }

            if (notifier is not null)
            {
                await notifier.BroadcastServicePingAsync(service.Id, result.EvaluatedStatus, result.LatencyMs, ct);
            }
        }

        await dbContext.SaveChangesAsync(ct);

        if (hasStatusChange && notifier is not null)
        {
            var overview = await mediator.Send(new GetSystemStatusQuery(), ct);
            await notifier.BroadcastStatusUpdateAsync(overview, ct);
        }
    }

    private static async Task<HealthCheckExecutionResult> PingServiceAsync(
        MonitoredService service,
        HttpClient client,
        CancellationToken ct)
    {
        var timeout = service.TimeoutMs > 0
            ? TimeSpan.FromMilliseconds(service.TimeoutMs)
            : TimeSpan.FromSeconds(10);

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        var stopwatch = Stopwatch.StartNew();
        int? statusCode = null;
        var isSuccess = false;
        var checkedAtUtc = DateTime.UtcNow;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, service.TargetUrl);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);

            stopwatch.Stop();
            statusCode = (int)response.StatusCode;
            isSuccess = response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            stopwatch.Stop();
            isSuccess = false;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            isSuccess = false;
        }

        var latencyMs = stopwatch.ElapsedMilliseconds;

        var evaluatedStatus = isSuccess
            ? (latencyMs > 1000 ? ServiceStatus.Degraded : ServiceStatus.Operational)
            : ServiceStatus.Down;

        return new HealthCheckExecutionResult(
            service,
            statusCode,
            latencyMs,
            isSuccess,
            evaluatedStatus,
            checkedAtUtc);
    }

    private sealed record HealthCheckExecutionResult(
        MonitoredService Service,
        int? StatusCode,
        long LatencyMs,
        bool IsSuccess,
        ServiceStatus EvaluatedStatus,
        DateTime CheckedAtUtc);
}
