using System.Reflection;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderAccumulator.Services;

public class FixSessionManager
{
    private readonly ILogger<FixSessionManager> _logger;
    private readonly IApplication _fixApplication;
    private ThreadedSocketAcceptor? _acceptor;
    
    public FixSessionManager(ILogger<FixSessionManager> logger,
                             IApplication fixApplication)
    {
        _logger = logger;
        _fixApplication = fixApplication;
    }

    public void StartSession()
    {
        try
        {
            var settingsFile = GetSettingsFile();
            var settings = new SessionSettings(settingsFile);
            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);

            _acceptor = new ThreadedSocketAcceptor(_fixApplication, storeFactory, settings, logFactory);
            _acceptor.Start();
            _logger.LogInformation("FIX session started.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting FIX session: {ex.Message}", ex);
        }
    }

    public void StopSession()
    {
        _acceptor?.Stop();
        _logger.LogInformation("FIX session terminated.");
    }
    
    private static string GetSettingsFile()
    {
        var runDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        var settingsFile = Path.Combine(runDir!, "FixConfig", "fix.cfg");
        return settingsFile;
    }
}