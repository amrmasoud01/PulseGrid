namespace PulseGrid.Application.Incidents.Commands.CreateIncident;

using MediatR;
using PulseGrid.Domain.Enums;

public record CreateIncidentCommand(
    Guid ServiceId,
    string Title,
    IncidentSeverity Severity,
    string? InitialMessage = null) : IRequest<Guid>;
