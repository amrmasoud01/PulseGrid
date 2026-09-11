using MediatR;
using PulseGrid.Api.Hubs;
using PulseGrid.Api.Services;
using PulseGrid.Application;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Application.Services.Commands.CreateMonitoredService;
using PulseGrid.Application.Services.Commands.DeleteMonitoredService;
using PulseGrid.Application.Services.Commands.ToggleServiceMonitoring;
using PulseGrid.Application.SystemStatus.Queries.GetSystemStatus;
using PulseGrid.BackgroundJobs;
using PulseGrid.Infrastructure;
using PulseGrid.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IStatusNotifier, StatusNotifier>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<ServiceMonitoringWorker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapOpenApi();
app.UseCors();

app.MapHub<StatusHub>("/hubs/status");

app.MapGet("/api/status", async (ISender sender, CancellationToken ct) =>
{
    var status = await sender.Send(new GetSystemStatusQuery(), ct);
    return Results.Ok(status);
})
.WithName("GetSystemStatus");

app.MapPost("/api/services", async (CreateMonitoredServiceCommand command, ISender sender, CancellationToken ct) =>
{
    var serviceId = await sender.Send(command, ct);
    return Results.Created($"/api/services/{serviceId}", new { id = serviceId });
})
.WithName("CreateMonitoredService");

app.MapDelete("/api/services/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
{
    await sender.Send(new DeleteMonitoredServiceCommand(id), ct);
    return Results.NoContent();
})
.WithName("DeleteMonitoredService");

app.MapPut("/api/services/{id:guid}/toggle", async (Guid id, ISender sender, CancellationToken ct) =>
{
    var isActive = await sender.Send(new ToggleServiceMonitoringCommand(id), ct);
    return Results.Ok(new { isActive });
})
.WithName("ToggleServiceMonitoring");

app.Run();
