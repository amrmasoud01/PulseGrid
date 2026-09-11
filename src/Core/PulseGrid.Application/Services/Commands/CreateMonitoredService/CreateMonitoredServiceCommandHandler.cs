namespace PulseGrid.Application.Services.Commands.CreateMonitoredService;

using MediatR;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Domain.Entities;
using PulseGrid.Domain.Enums;

public class CreateMonitoredServiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateMonitoredServiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateMonitoredServiceCommand request, CancellationToken cancellationToken)
    {
        var service = new MonitoredService
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            TargetUrl = request.TargetUrl,
            IntervalSeconds = request.IntervalSeconds,
            TimeoutMs = request.TimeoutMs,
            CurrentStatus = ServiceStatus.Operational,
            LastCheckedAtUtc = null
        };

        context.MonitoredServices.Add(service);
        await context.SaveChangesAsync(cancellationToken);

        return service.Id;
    }
}
