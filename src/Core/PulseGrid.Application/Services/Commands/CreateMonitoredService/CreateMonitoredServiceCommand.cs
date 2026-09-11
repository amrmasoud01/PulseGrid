namespace PulseGrid.Application.Services.Commands.CreateMonitoredService;

using MediatR;

public record CreateMonitoredServiceCommand(
    string Name,
    string TargetUrl,
    int IntervalSeconds,
    int TimeoutMs) : IRequest<Guid>;
