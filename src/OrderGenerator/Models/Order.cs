namespace OrderGenerator.Models;

public class Order
{
    public string? OrderId { get; set; }
    public string? ExecId { get; set; }
    public string? ClOrdId { get; set; }
    public string? Symbol { get; set; } 
    public OrderSide Side { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; }
    public string? Account { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RejectionReason { get; set; }
}