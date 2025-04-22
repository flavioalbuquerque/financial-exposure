using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrderAccumulator.Middlewares;
using OrderAccumulator.Services;
using OrderAccumulator.Services.Interfaces;
using OrderAccumulator.Settings;
using QuickFix;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;

namespace OrderAccumulator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Logging.AddConsole();
        
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracingBuilder => tracingBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter()
            )
            .WithMetrics(metricsBuilder => metricsBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter()
            );
        
        builder.Services.Configure<ExposureSettings>( builder.Configuration.GetSection("ExposureSettings"));
        builder.Services.AddSingleton<ExposureService>(); 
        
        builder.Services.AddSingleton<IExposureService, ExposureService>();

        builder.Services.AddSingleton<IApplication, FixApplication>();
        builder.Services.AddSingleton<FixSessionManager>();
        
        builder.Services.AddHostedService<FixBackgroundService>();
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
        
        builder.Services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Order Accumulator API",
                Version = "v1",
                Description = "API para consultar e gerenciar ordens limite de exposição financeira."
            });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });
        
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.Map("/openapi/v1.json", async context =>
            {
                var swaggerProvider = app.Services.GetRequiredService<ISwaggerProvider>();
                var swaggerDoc = swaggerProvider.GetSwagger("v1");

                context.Response.ContentType = "application/json";

                await using var writer = new StringWriter();
                var openApiWriter = new Microsoft.OpenApi.Writers.OpenApiJsonWriter(writer);
                swaggerDoc.SerializeAsV3(openApiWriter);
                openApiWriter.Flush();

                var json = writer.ToString();
                await context.Response.WriteAsync(json);
            });
            
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderAccumulator V1");
            });

            app.UseReDoc(options =>
            {
                options.RoutePrefix = "redoc";
                options.SpecUrl = "/openapi/v1.json";
                options.DocumentTitle = "API - OrderAccumulator";
            });
            
            app.MapScalarApiReference();
        }

        app.UseAuthorization();
        
        app.MapControllers();
        
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"Healthy\"}");
            }
        });
        
        app.Run();
    }
}