using BoaSorte.Api.Endpoints;
using BoaSorte.Api.Http;
using BoaSorte.Application;
using BoaSorte.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddApiRateLimiting(builder.Configuration);

// TLS terminates at Traefik (Dokploy); trust its X-Forwarded-* headers.
// Its container IP is not stable, so the known proxy/network lists are cleared.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.Services.MigrateDatabaseAsync();

app.MapHealthChecks("/api/health");
app.MapDrawEndpoints();

await app.RunAsync();

public partial class Program;
