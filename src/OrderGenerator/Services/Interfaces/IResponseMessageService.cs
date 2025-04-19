namespace OrderGenerator.Services.Interfaces;

public interface IResponseMessageService
{
    event Action<string>? OnMessageUpdated;
    void UpdateMessage(string message);
}