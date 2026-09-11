using MediatR;
using PulseGrid.Api.Hubs;
using PulseGrid.Api.Services;
using PulseGrid.Application;
using PulseGrid.Application.Common.Interfaces;
using PulseGrid.Application.Services.Commands.CreateMonitoredService;
using PulseGrid.Application.SystemStatus.Queries.GetSystemStatus;
using PulseGrid.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IStatusNotifier, StatusNotifier>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
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

app.Run();
