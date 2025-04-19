using System.Reflection;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderGenerator.Services;

public class FixSessionManager
{
    private readonly ILogger<FixSessionManager> _logger;
    private readonly IApplication _application;
    private SocketInitiator? _initiator;
    
    public FixSessionManager(ILogger<FixSessionManager> logger,
                             IApplication application)
    {
        _logger = logger;
        _application = application;
    }

    public void StartSession()
    {
        try
        {
            var settings = new SessionSettings(GetSettingsFile());
            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);

            _initiator = new SocketInitiator(_application, storeFactory, settings, logFactory);
            _initiator.Start();
            
            _logger.LogInformation("FIX session started.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting FIX session: {ex.Message}", ex);
        }
    }

    public void StopSession()
    {
        // Parar a sessão FIX
        _initiator?.Stop();
        _logger.LogInformation("FIX session terminated.");
    }
    
    private static string GetSettingsFile()
    {
        var runDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        var settingsFile = Path.Combine(runDir!, "FixConfig", "fix.cfg");
        return settingsFile;
    }
}