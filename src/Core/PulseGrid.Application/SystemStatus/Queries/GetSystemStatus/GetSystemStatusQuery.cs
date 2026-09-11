namespace PulseGrid.Application.SystemStatus.Queries.GetSystemStatus;

using MediatR;
using PulseGrid.Application.Common.Models;

public record GetSystemStatusQuery : IRequest<SystemStatusOverviewDto>;
