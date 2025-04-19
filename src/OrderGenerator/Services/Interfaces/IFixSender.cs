using OrderGenerator.Models;

namespace OrderGenerator.Services.Interfaces;

public interface IFixSender
{
    bool SendNewOrderSingle(Order order);
}