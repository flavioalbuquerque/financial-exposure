using OrderGenerator.Models;

namespace OrderGenerator.Services.Interfaces;

public interface IOrderService
{
    Task<string> SendOrder(OrderModel orderModel);
    IEnumerable<string> GetSymbols();
    IEnumerable<(string Value, string Text)> GetSides();
    string GetPriceStep();
}