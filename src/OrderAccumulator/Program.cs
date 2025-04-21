using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrderAccumulator.Services;
using OrderAccumulator.Settings;
using QuickFix;

namespace OrderAccumulator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
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
        
        builder.Services.AddSingleton<ExposureCalculatorService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ExposureCalculatorService>>();
            var settings = provider.GetRequiredService<IOptions<ExposureSettings>>().Value;
            return new ExposureCalculatorService(logger, settings.DefaultMaxExposure);
        });

        builder.Services.AddSingleton<IApplication, FixApplication>();
        builder.Services.AddSingleton<FixSessionManager>();
        
        builder.Services.AddHostedService<FixBackgroundService>();
    
        var host = builder.Build();
        host.Run();
    }
}