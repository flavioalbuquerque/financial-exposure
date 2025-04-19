using OrderAccumulator.Models;

namespace OrderAccumulator.Services;

public class ExposureCalculatorService
{
    private readonly ILogger<ExposureCalculatorService> _logger;
    private readonly Dictionary<string, List<Order>> _ordersBySymbol = new();

    private readonly decimal _defaultMaxExposure;
    
    public ExposureCalculatorService(ILogger<ExposureCalculatorService> logger,
                                     decimal defaultMaxExposure)
    {
        _logger = logger;
        _defaultMaxExposure = defaultMaxExposure;
    }
    
    public bool CalculateExposureAddOrderIfWithinExposureLimit(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.Symbol) || order.Quantity <= 0)
            return false;

        var currentExposure = GetExposure(order.Symbol);

        var amount = order.Side switch
        {
            OrderSide.Buy => order.Amount,
            OrderSide.Sell => -order.Amount,
            _ => 0
        };

        var newExposure = currentExposure + amount;

        _logger.LogDebug($"{nameof(Order)} Symbol: {order.Symbol}, Amount: {amount}, Side: {order.Side}, Current Exposure: {currentExposure}, New Exposure: {newExposure}, Max Exposure: {_defaultMaxExposure}");

        if (Math.Abs(newExposure) > _defaultMaxExposure)
        {
            _logger.LogDebug($"Order exceeds the configured exposure limit.");
            return false;
        }
        
        _logger.LogDebug($"Order is within the configured exposure limit.");

        if (!_ordersBySymbol.ContainsKey(order.Symbol))
            _ordersBySymbol[order.Symbol] = new List<Order>();

        _ordersBySymbol[order.Symbol].Add(order);
        
        return true;
    }

    private decimal GetExposure(string symbol)
    {
        if (!_ordersBySymbol.TryGetValue(symbol, out var orders))
            return 0;

        return orders.Sum(o =>
        {
            var value = o.Quantity * o.Price;
            return o.Side switch
            {
                OrderSide.Buy => value,
                OrderSide.Sell => -value,
                _ => 0
            };
        });
    }
}
