using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrderAccumulator.Middlewares;
using OrderAccumulator.Services;
using OrderAccumulator.Services.Interfaces;
using OrderAccumulator.Settings;
using QuickFix;
using Scalar.AspNetCore;

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

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "OrderAccumulator V1");
            });

            app.UseReDoc(options =>
            {
                options.SpecUrl("/openapi/v1.json");
            });
            
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
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