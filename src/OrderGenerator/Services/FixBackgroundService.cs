namespace OrderGenerator.Services;

public class FixBackgroundService : BackgroundService
{
    private readonly ILogger<FixBackgroundService> _logger;
    private readonly FixSessionManager _fixSessionManager;

    public FixBackgroundService(ILogger<FixBackgroundService> logger,
                                FixSessionManager fixSessionManager)
    {
        _logger = logger;
        _fixSessionManager = fixSessionManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Background service...");

            _fixSessionManager.StartSession();
            
            _logger.LogInformation("Background service started successfully.");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                // _logger.LogDebug("Background service running at: {time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Background service has been canceled.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Background service...");
        _fixSessionManager.StopSession();
        
        await base.StopAsync(cancellationToken);
    }
    
    public override void Dispose()
    {
        _logger.LogInformation("Disposing Background service...");
        _fixSessionManager.StopSession();
        
        base.Dispose();
    }

}