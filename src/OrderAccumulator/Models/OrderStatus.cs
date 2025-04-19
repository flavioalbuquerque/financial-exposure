namespace OrderAccumulator.Models;

public enum OrderStatus
{
    Undefined = 0,
    New = 1,
    PartiallyFilled = 3,
    Filled = 4,
    Canceled = 5,
    Rejected = 6,
    Expired = 7
}