using OrderAccumulator.Models;

namespace OrderAccumulator.UnitTests.Builders;

public class OrderBuilder
{
    private string _symbol = string.Empty;
    private decimal _quantity = 0;
    private decimal _price = 0;
    private OrderSide _side = OrderSide.Undefined;

    public OrderBuilder WithSymbol(string symbol)
    {
        _symbol = symbol;
        return this;
    }

    public OrderBuilder WithQuantity(decimal quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public OrderBuilder WithSide(OrderSide side)
    {
        _side = side;
        return this;
    }

    public Order Build()
    {
        return new Order
        {
            ClOrdId = Guid.NewGuid().ToString(),
            Symbol = _symbol,
            Quantity = _quantity,
            Price = _price,
            Side = _side
        };
    }
}