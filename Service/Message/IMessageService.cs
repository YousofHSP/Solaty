namespace Service.Message
{
    public interface IMessageService
    {
        Task<bool> SendMessageAsync(string phoneNumber, string message, long? userId, long? creatorUserId, CancellationToken ct);
    }
}
