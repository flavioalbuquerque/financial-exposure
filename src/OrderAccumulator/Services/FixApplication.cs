using OrderAccumulator.Extensions;
using OrderAccumulator.Models;
using QuickFix;

namespace OrderAccumulator.Services
{
    public class FixApplication : MessageCracker, IApplication
    {
        private readonly ILogger<FixApplication> _logger;
        private readonly ExposureCalculatorService _exposureCalculatorService;
        
        public FixApplication(ILogger<FixApplication> logger,
                              ExposureCalculatorService exposureCalculatorService)
        {
            _logger = logger;
            _exposureCalculatorService = exposureCalculatorService;
        }

        private string? GenOrderId(bool orderCreated)
        {
            return orderCreated ? Guid.NewGuid().ToString() : null;
        }

        private string? GenExecId()
        {
            return Guid.NewGuid().ToString();
        }

        #region QuickFix.Application Methods

        public void FromApp(Message message, SessionID sessionId)
        {
            _logger.LogDebug($"FromApp (In): {message}");
            Crack(message, sessionId);
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            _logger.LogDebug($"ToApp (Out): {message}");
        }

        public void FromAdmin(Message message, SessionID sessionId)
        {
            _logger.LogDebug($"FromAdmin: {message}");
        }

        public void ToAdmin(Message message, SessionID sessionId)
        {
            _logger.LogDebug($"ToAdmin: {message}");
        }

        public void OnCreate(SessionID sessionId)
        {
            _logger.LogDebug($"OnCreate: {sessionId}");
        }

        public void OnLogout(SessionID sessionId)
        {
            _logger.LogDebug($"OnLogout: {sessionId}");
        }

        public void OnLogon(SessionID sessionId)
        {
            _logger.LogDebug($"OnLogon: {sessionId}");
        }
        
        #endregion

        #region MessageCracker overloads

        public void OnMessage(QuickFix.FIX44.NewOrderSingle n, SessionID sessionId)
        {
            var order = n.ToOrder("OrderGenerator");

            var added = _exposureCalculatorService.CalculateExposureAddOrderIfWithinExposureLimit(order);
            
            order.OrderId = GenOrderId(added);
            order.ExecId = GenExecId();
            order.Status = added ? OrderStatus.New : OrderStatus.Rejected;
            order.RejectionReason = added ? null : "Ordem excedeu o limite de exposição financeira";

            var executionReport = order.ToExecutionReport();

            try
            {
                Session.SendToTarget(executionReport, sessionId);
            }
            catch (SessionNotFound ex)
            {
                _logger.LogError($"Session not found: [{sessionId}]", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
