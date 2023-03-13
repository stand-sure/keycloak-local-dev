using System.Reflection;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Prometheus;

using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((context, provider, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(provider));

builder.Services.AddLogging();

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(options => { options.AddPolicy("user", policyBuilder => policyBuilder.RequireClaim("user")); });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    options.CustomSchemaIds(type => type.FullName);
});

builder.Services.AddHealthChecks();

string serviceName = builder.Environment.ApplicationName;
var version = typeof(Program).GetTypeInfo().Assembly.GetName().Version?.ToString();
ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName, version);

builder.Services.AddOpenTelemetry().WithTracing(providerBuilder => { ConfigureTracing(providerBuilder, serviceName, resourceBuilder, builder); });

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseHttpMetrics();

app.UseAuthorization();

app.MapHealthChecks("healthz/ready",
    new HealthCheckOptions()
    {
        Predicate = _ => false,
    });

app.MapHealthChecks("healthz/live",
    new HealthCheckOptions()
    {
        Predicate = _ => false,
    });

app.MapMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

static void ConfigureTracing(
    TracerProviderBuilder tracerProviderBuilder,
    string source,
    ResourceBuilder resourceBuilder,
    WebApplicationBuilder webApplicationBuilder)
{
    tracerProviderBuilder.AddSource(source).SetResourceBuilder(resourceBuilder);
    tracerProviderBuilder.AddHttpClientInstrumentation();
    tracerProviderBuilder.AddAspNetCoreInstrumentation();

    tracerProviderBuilder.AddJaegerExporter(options => { ConfigureJaegerExporter(webApplicationBuilder, options); });
}

static void ConfigureJaegerExporter(WebApplicationBuilder webApplicationBuilder, JaegerExporterOptions jaegerExporterOptions)
{
    string? endpointUriAddress = webApplicationBuilder.Configuration["JaegerExporter:EndpointUri"];

    bool goodUri = Uri.TryCreate(endpointUriAddress, UriKind.Absolute, out Uri? endpointUri);

    if (goodUri is false)
    {
        return;
    }

    jaegerExporterOptions.Endpoint = endpointUri;
    jaegerExporterOptions.Protocol = JaegerExportProtocol.HttpBinaryThrift;
}