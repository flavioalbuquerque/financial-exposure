using OrderGenerator.Models;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderGenerator.Extensions;

public static class OrderExtensions
{
    private static OrderSide GetOrderSide(string side) =>
        side switch
        {
            "BUY" => OrderSide.Buy, 
            "SELL" => OrderSide.Sell,
            _ => OrderSide.Undefined
        };
    
    public static Order ToOrderFix(this OrderModel orderModel, string clOrdId)
    {
        if (string.IsNullOrWhiteSpace(clOrdId))
            throw new ArgumentException("ClOrdId cannot be empty.", nameof(clOrdId));
        
        if (orderModel.Quantity.GetValueOrDefault() == 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(orderModel.Quantity));
        
        if (orderModel.Price.GetValueOrDefault() == 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(orderModel.Price));
        
        var side = GetOrderSide(orderModel.Side);
        var order = new Order
        {
            ClOrdId = clOrdId,
            Symbol = orderModel.Symbol,
            Side = side,
            Quantity = orderModel.Quantity.GetValueOrDefault(),
            Price = orderModel.Price.GetValueOrDefault(),
            Type = OrderType.Limit,
            Status = OrderStatus.PendingNew,
            CreatedAt = DateTime.UtcNow
        };

        return order;
    }
    
    public static NewOrderSingle ToNewOrderSingle(this Order order)
    {
        if (string.IsNullOrWhiteSpace(order.ClOrdId))
            throw new ArgumentException("ClOrdId cannot be empty.", nameof(order.ClOrdId));
        
        if (string.IsNullOrWhiteSpace(order.Symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(order.Symbol));
        
        var newOrder = new NewOrderSingle(
            new ClOrdID(order.ClOrdId),
            new Symbol(order.Symbol),
            order.Side.ToSide(),
            new TransactTime(order.CreatedAt),
            new OrdType(OrdType.LIMIT)
        )
        {
            OrderQty = new OrderQty(order.Quantity),
            Price = new Price(order.Price),
        };

        return newOrder;
    }
    
    private static Side ToSide(this OrderSide orderSide) =>
        orderSide switch
        {
            OrderSide.Buy => new Side(Side.BUY),
            OrderSide.Sell => new Side(Side.SELL),
            OrderSide.Undefined => throw new ArgumentException("Order side cannot be Undefined", nameof(orderSide)),
            _ => throw new ArgumentException($"Order side not supported: [{orderSide}]", nameof(orderSide))
        };
    
    public static Order ToOrder(this ExecutionReport execReport)
    {
        ArgumentNullException.ThrowIfNull(execReport);

        var order = new Order
        {
            OrderId = execReport.OrderID.Value,
            ExecId = execReport.ExecID.Value,
            ClOrdId = execReport.ClOrdID.Value,
            Symbol = execReport.Symbol.Value,
            Side = execReport.Side.ToOrderSide(),
            Quantity = execReport.OrderQty.Value,
            Price = execReport.IsSetPrice() ? execReport.Price.Value : execReport.AvgPx.Value,
            Status = execReport.OrdStatus.ToOrderStatus()
        };

        if (execReport.IsSetOrdType())
            order.Type = execReport.OrdType.ToOrderType();
        
        if(execReport.IsSetAccount())
            order.Account = execReport.Account.Value;
        
        if(execReport.IsSetTransactTime())
            order.CreatedAt = execReport.TransactTime.Value;

        if (execReport.IsSetText())
            order.RejectionReason = execReport.Text.Value;
        
        return order;
    }

private static OrderSide ToOrderSide(this Side side) =>
    side.Value switch
    {
        Side.BUY => OrderSide.Buy,
        Side.SELL => OrderSide.Sell,
        _ => throw new ArgumentException($"Side not supported: [{side}]", nameof(side))
    };

private static OrderType ToOrderType(this OrdType ordType) =>
    ordType.Value switch
    {
        OrdType.MARKET => OrderType.Market,
        OrdType.LIMIT => OrderType.Limit,
        OrdType.STOP => OrderType.Stop,
        OrdType.STOP_LIMIT => OrderType.StopLimit,
        _ => OrderType.Undefined
    };

private static OrderStatus ToOrderStatus(this OrdStatus ordStatus) =>
    ordStatus.Value switch
    {
        OrdStatus.NEW => OrderStatus.New,
        OrdStatus.REJECTED => OrderStatus.Rejected,
        OrdStatus.PARTIALLY_FILLED => OrderStatus.PartiallyFilled,
        OrdStatus.FILLED => OrderStatus.Filled,
        OrdStatus.CANCELED => OrderStatus.Canceled,
        OrdStatus.EXPIRED => OrderStatus.Expired,
        _ => throw new ArgumentException($"Order status not supported: [{ordStatus.Value}]", nameof(ordStatus))
    };


}