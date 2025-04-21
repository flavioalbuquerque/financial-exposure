using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrderGenerator.Components;
using OrderGenerator.Services;
using OrderGenerator.Services.Interfaces;
using QuickFix;

namespace OrderGenerator;

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
        
        builder.Services.AddSingleton<IResponseMessageService, ResponseMessageService>();

        builder.Services.AddSingleton<FixApplication>();
        builder.Services.AddSingleton<IApplication>(sp => sp.GetRequiredService<FixApplication>());
        builder.Services.AddSingleton<IFixSender>(sp => sp.GetRequiredService<FixApplication>());
        
        builder.Services.AddSingleton<FixSessionManager>();
        
        builder.Services.AddScoped<IOrderService, OrderService>();
        
        builder.Services.AddHostedService<FixBackgroundService>();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        var app = builder.Build();

        app.UseAntiforgery();
        app.MapStaticAssets();
        
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}