using OrderGenerator.Extensions;
using OrderGenerator.Models;
using OrderGenerator.Services.Interfaces;

namespace OrderGenerator.Services;

public class OrderService : IOrderService
{
    private readonly IFixSender _fixSender;

    public OrderService(IFixSender fixSender)
    {
        _fixSender = fixSender;
    }
    
    public Task<string> SendOrder(OrderModel orderModel)
    {
        var clOrdId = Guid.NewGuid().ToString();
        var order = orderModel.ToOrderFix(clOrdId);

        var sent = _fixSender.SendNewOrderSingle(order);
        var message = sent ? "Aguardando retorno..." : "Erro no envio";
        return Task.FromResult(message);
    }
    
    public IEnumerable<string> GetSymbols()
    {
        return new[] { "PETR4", "VALE3", "VIIA4" };
    }

    public IEnumerable<(string Value, string Text)> GetSides()
    {
        return new[]
        {
            ("BUY", "Compra"),
            ("SELL", "Venda")
        };
    }
    
    public string GetPriceStep()
    {
        return "0.01";
    }
}