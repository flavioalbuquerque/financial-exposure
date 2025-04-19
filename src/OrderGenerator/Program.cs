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
        
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        builder.Services.AddSingleton<IResponseMessageService, ResponseMessageService>();

        builder.Services.AddSingleton<FixApplication>();
        builder.Services.AddSingleton<IApplication>(sp => sp.GetRequiredService<FixApplication>());
        builder.Services.AddSingleton<IFixSender>(sp => sp.GetRequiredService<FixApplication>());

        
        // builder.Services.AddSingleton<IApplication, FixApplication>();
        // builder.Services.AddSingleton<IFixSender, FixApplication>();
        // builder.Services.AddSingleton<IFixSender, FixSender>();
        builder.Services.AddSingleton<FixSessionManager>();
        
        builder.Services.AddScoped<IOrderService, OrderService>();
        
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();
        
        // Inicializa a sessão FIX
        var sessionManager = app.Services.GetRequiredService<FixSessionManager>();
        sessionManager.StartSession();
        
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}