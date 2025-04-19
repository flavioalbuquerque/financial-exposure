using OrderGenerator.Services.Interfaces;

namespace OrderGenerator.Services;

public class ResponseMessageService : IResponseMessageService
{
    public event Action<string>? OnMessageUpdated;

    public void UpdateMessage(string message)
    {
        OnMessageUpdated?.Invoke(message);
    }
}
