namespace OrderGenerator.Models;

public enum OrderType
{
    Undefined = 0,
    Market = 1,
    Limit = 2,
    Stop = 3,
    StopLimit = 4
}