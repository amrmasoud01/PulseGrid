namespace PulseGrid.Application.Services.Commands.DeleteMonitoredService;

using MediatR;

public record DeleteMonitoredServiceCommand(Guid Id) : IRequest;
