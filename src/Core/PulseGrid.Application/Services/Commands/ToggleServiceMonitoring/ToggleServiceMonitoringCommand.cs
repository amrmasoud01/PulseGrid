namespace PulseGrid.Application.Services.Commands.ToggleServiceMonitoring;

using MediatR;

public record ToggleServiceMonitoringCommand(Guid Id) : IRequest<bool>;
