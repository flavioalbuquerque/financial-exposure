namespace OrderAccumulator.Models;

public class Order
{
    public string? OrderId { get; set; }
    public string? ExecId { get; set; }
    public required string ClOrdId { get; init; }
    public required string Symbol { get; init; } 
    public OrderSide Side { get; init; }
    public decimal Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal Amount => Quantity * Price;
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; }
    public string? Account { get; set; }
    public string? Source { get; set; }
    public DateTime TransactTime { get; set; }
    public string? RejectionReason { get; set; }
}