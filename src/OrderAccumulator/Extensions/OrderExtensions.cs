using OrderAccumulator.Models;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Extensions;

public static class OrderExtensions
{
    public static Order ToOrder(this NewOrderSingle newOrder, string source)
    {
        ArgumentNullException.ThrowIfNull(newOrder);
        
        var order = new Order
        {
            ClOrdId = newOrder.ClOrdID.Value,
            Symbol = newOrder.Symbol.Value,
            Quantity = newOrder.OrderQty.Value,
            Price = newOrder.Price.Value,
            Side = newOrder.Side.ToOrderSide(),
            TransactTime = newOrder.TransactTime.Value,
            Source = source
        };

        if (newOrder.IsSetOrdType())
            order.Type = newOrder.OrdType.ToOrderType();
        
        if(newOrder.IsSetAccount())
            order.Account = newOrder.Account.Value;
        
        return order;
    }
    
    private static OrderSide ToOrderSide(this Side side) =>
        side.Value switch
        {
            Side.BUY => OrderSide.Buy,
            Side.SELL => OrderSide.Sell,
            _ => OrderSide.Undefined
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

    public static ExecutionReport ToExecutionReport(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var executionReport = new ExecutionReport(
            new OrderID(order.OrderId ?? 0.ToString()),
            new ExecID(order.ExecId ?? 0.ToString()),
            order.Status.ToExecType(),
            order.Status.ToOrdStatus(),
            new Symbol(order.Symbol),
            order.Side.ToSide(),
            new LeavesQty(order.Quantity),
            new CumQty(0),
            new AvgPx(order.Price)
        );

        executionReport.Set(new ClOrdID(order.ClOrdId));
        executionReport.Set(new OrderQty(order.Quantity));
        executionReport.Set(new Price(order.Price));

        if (order.Type != OrderType.Undefined)
        {
            executionReport.Set(order.Type.ToOrdType());        
        }
        
        if (!string.IsNullOrWhiteSpace(order.Account))
        {
            executionReport.Set(new Account(order.Account));        
        }
        
        if (order.Status == OrderStatus.Rejected && !string.IsNullOrWhiteSpace(order.RejectionReason))
        {
            executionReport.Set(new Text(order.RejectionReason));
        }

        if (order.TransactTime != default)
        {
            executionReport.Set(new TransactTime(order.TransactTime));
        }
            
        return executionReport;
    }

    private static ExecType ToExecType(this OrderStatus orderStatus) =>
        orderStatus switch
        {
            OrderStatus.New => new ExecType(ExecType.NEW),
            OrderStatus.Rejected => new ExecType(ExecType.REJECTED),
            OrderStatus.PartiallyFilled => new ExecType(ExecType.PARTIAL_FILL),
            OrderStatus.Filled => new ExecType(ExecType.FILL),
            OrderStatus.Canceled => new ExecType(ExecType.CANCELED),
            OrderStatus.Expired => new ExecType(ExecType.EXPIRED),
            OrderStatus.Undefined => throw new ArgumentException("Order status cannot be Undefined", nameof(orderStatus)),
            _ => throw new ArgumentException($"Order status not supported: [{orderStatus}]", nameof(orderStatus))
        };
    
    private static OrdStatus ToOrdStatus(this OrderStatus orderStatus) =>
        orderStatus switch
        {
            OrderStatus.New => new OrdStatus(OrdStatus.NEW),
            OrderStatus.Rejected => new OrdStatus(OrdStatus.REJECTED),
            OrderStatus.PartiallyFilled => new OrdStatus(OrdStatus.PARTIALLY_FILLED),
            OrderStatus.Filled => new OrdStatus(OrdStatus.FILLED),
            OrderStatus.Canceled => new OrdStatus(OrdStatus.CANCELED),
            OrderStatus.Expired => new OrdStatus(OrdStatus.EXPIRED),
            OrderStatus.Undefined => throw new ArgumentException("Order status cannot be Undefined", nameof(orderStatus)),
            _ => throw new ArgumentException($"Order status not supported: [{orderStatus}]", nameof(orderStatus))
        };
    
    private static OrdType ToOrdType(this OrderType orderType) =>
        orderType switch
        {
            OrderType.Market => new OrdType(OrdType.MARKET),
            OrderType.Limit => new OrdType(OrdType.LIMIT),
            OrderType.Stop => new OrdType(OrdType.STOP),
            OrderType.StopLimit => new OrdType(OrdType.STOP_LIMIT),
            OrderType.Undefined => throw new ArgumentException($"Order type cannot be Undefined: [{orderType}]", nameof(orderType)),
            _ => throw new ArgumentException($"Order type not supported: [{orderType}]", nameof(orderType))
        };
    private static Side ToSide(this OrderSide orderSide) =>
        orderSide switch
        {
            OrderSide.Buy => new Side(Side.BUY),
            OrderSide.Sell => new Side(Side.SELL),
            OrderSide.Undefined => throw new ArgumentException("Order side cannot be Undefined", nameof(orderSide)),
            _ => throw new ArgumentException($"Order side not supported: [{orderSide}]", nameof(orderSide))
        };
}