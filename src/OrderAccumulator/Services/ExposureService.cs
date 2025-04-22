using Microsoft.Extensions.Options;
using OrderAccumulator.Exceptions;
using OrderAccumulator.Models;
using OrderAccumulator.Services.Interfaces;
using OrderAccumulator.Settings;

namespace OrderAccumulator.Services;

public class ExposureService : IExposureService
{
    private readonly ILogger<ExposureService> _logger;
    private readonly Dictionary<string, List<Order>> _ordersBySymbol = new();

    private readonly ExposureSettings _exposureSettings;
    
    public ExposureService(ILogger<ExposureService> logger, 
                           IOptions<ExposureSettings> exposureSettings)
    {
        _logger = logger;
        _exposureSettings = exposureSettings.Value;
    }
    
    public bool CalculateExposureAddOrderIfWithinExposureLimit(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.Symbol))
            throw new BusinessValidationException("Symbol cannot be empty.", nameof(order.Symbol));
        
        if (order.Quantity <= 0)
            throw new BusinessValidationException("Quantity must be greater than zero.", nameof(order.Quantity));

        var currentExposure = GetExposure(order.Symbol);

        var amount = order.Side switch
        {
            OrderSide.Buy => order.Amount,
            OrderSide.Sell => -order.Amount,
            _ => 0
        };

        var newExposure = currentExposure + amount;
        var maxExposure = _exposureSettings.DefaultMaxExposure;
        _logger.LogDebug($"{nameof(Order)} Symbol: {order.Symbol}, Amount: {amount:#,0.00}, Side: {order.Side}, Current Exposure: {currentExposure:#,0.00}, New Exposure: {newExposure:#,0.00}, Max Exposure: {maxExposure:#,0.00}");

        if (Math.Abs(newExposure) > maxExposure)
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

    public decimal GetExposure(string symbol)
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
    
    public decimal GetDefaultMaxExposure()
    {
        return _exposureSettings.DefaultMaxExposure;
    }

    public bool SetDefaultMaxExposure(decimal defaultMaxExposure)
    {
        if(defaultMaxExposure <= 0)
            throw new BusinessValidationException("Default max exposure must be greater than zero.", nameof(defaultMaxExposure));
        
        _exposureSettings.DefaultMaxExposure = defaultMaxExposure;
        return true;
    }
    
    public Dictionary<string, decimal> GetAllExposures()
    {
        return _ordersBySymbol.ToDictionary(
            pair => pair.Key,
            pair => GetExposure(pair.Key)
        );
    }

    public IReadOnlyList<Order> GetOrders(string symbol)
    {
        return _ordersBySymbol.TryGetValue(symbol, out var list)
            ? list.AsReadOnly()
            : Array.Empty<Order>();
    }

    public IReadOnlyDictionary<string, List<Order>> GetAllOrders()
    {
        return _ordersBySymbol.ToDictionary(
            pair => pair.Key,
            pair => new List<Order>(pair.Value)
        );
    }

    public bool DeleteOrders(string symbol)
    {
        _ordersBySymbol.Remove(symbol);
        return true;
    }
    
    public bool DeleteAllOrders()
    {
        _ordersBySymbol.Clear();
        return true;
    }
}
