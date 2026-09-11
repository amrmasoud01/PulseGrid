using PulseGrid.Application;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.BackgroundJobs;
using PulseGrid.BackgroundJobs.Services;
using PulseGrid.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHttpClient("HealthCheckClient");
builder.Services.AddSingleton<IStatusNotifier, LoggingStatusNotifier>();
builder.Services.AddHostedService<ServiceMonitoringWorker>();

var host = builder.Build();
host.Run();
