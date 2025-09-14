namespace Service.Message
{
    public interface IMessageService
    {
        Task<bool> SendMessageAsync(string phoneNumber, string message, int? userId, int? creatorUserId, CancellationToken ct);
    }
}
