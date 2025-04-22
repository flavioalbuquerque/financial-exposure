using OrderAccumulator.Models;

namespace OrderAccumulator.Services.Interfaces;

public interface IExposureService
{
    bool CalculateExposureAddOrderIfWithinExposureLimit(Order order);

    decimal GetExposure(string symbol);
    
    decimal GetDefaultMaxExposure();
    
    void SetDefaultMaxExposure(decimal defaultMaxExposure);

    Dictionary<string, decimal> GetAllExposures();

    IReadOnlyList<Order> GetOrders(string symbol);

    IReadOnlyDictionary<string, List<Order>> GetAllOrders();

    void ResetAllOrders();
}