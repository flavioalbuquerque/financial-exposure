using OrderGenerator.Extensions;
using OrderGenerator.Models;
using OrderGenerator.Services.Interfaces;
using QuickFix;

namespace OrderGenerator.Services;

public class FixApplication : MessageCracker, IApplication, IFixSender
{
    private readonly ILogger<FixApplication> _logger;
    private readonly IResponseMessageService _responseMessageService;

    private SessionID? _sessionId;
    
    public FixApplication(ILogger<FixApplication> logger,
                          IResponseMessageService responseMessageService)
    {
        _logger = logger;
        _responseMessageService = responseMessageService;
    }
    
    #region IApplication interface
    
    public void OnCreate(SessionID sessionId)
    {
        _logger.LogDebug($"[OnCreate] {sessionId}");
        var session = Session.LookupSession(sessionId);
        if (session is null)
        {
            _sessionId = null;
            throw new ApplicationException($"Session not found for SessionId: {sessionId}");
        }
        
        _sessionId = sessionId;
    }

    public void OnLogon(SessionID sessionId)
    {
        _logger.LogDebug($"Logon: {sessionId}");
    }

    public void OnLogout(SessionID sessionId)
    {
        _logger.LogDebug($"Logout: {sessionId}");
        _sessionId = null;
    }
    
    public void FromAdmin(Message message, SessionID sessionId)
    {
        _logger.LogDebug($"FromAdmin: {message}");
    }
    
    public void ToAdmin(Message message, SessionID sessionId)
    {
        _logger.LogDebug($"ToAdmin: {message}");
    }

    public void FromApp(Message message, SessionID sessionId)
    {
        _logger.LogDebug($"FromApp (In): {message}");
        Crack(message,sessionId);
    }

    public void ToApp(Message message, SessionID sessionId)
    {
        _logger.LogDebug($"ToApp (Out): {message}");
    }

    #endregion
    
    #region MessageCracker handlers
    
    public void OnMessage(QuickFix.FIX44.ExecutionReport message, SessionID sessionId)
    {
        _logger.LogDebug($"Received execution report: {message}");
        
        var order = message.ToOrder();
        var responseMessage = GetResponseMessage(order);
        
        _responseMessageService.UpdateMessage(responseMessage);
    }
    
    private static string GetResponseMessage(Order order) =>
        order.Status switch
        {
            OrderStatus.New => "Ordem criada com sucesso",
            OrderStatus.Rejected => !string.IsNullOrWhiteSpace(order.RejectionReason)
                ? $"Ordem rejeitada. Motivo: {order.RejectionReason}"
                : "Ordem rejeitada",
            _ => $"Erro ao receber mensagem de retorno. Status desconhecido: [{order.Status.ToString()}]"
        };
    
    #endregion
    
    public bool SendNewOrderSingle(Order order)
    {
        if(_sessionId is null)
            throw new ApplicationException("Session Id is null");
        
        var newOrder = order.ToNewOrderSingle();
        
        var sent = Session.SendToTarget(newOrder, _sessionId);
        return sent;
    }
}