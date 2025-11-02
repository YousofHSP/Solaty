using System.Net.Http.Json;
using Data.Contracts;
using Domain.Entities;
using Domain.Message;

namespace Service.Message
{
    public class KavehNegarMessageService : IMessageService
    {
        private readonly HttpClient _httpClient;
        private readonly IRepository<SmsLog> _repository;

        public KavehNegarMessageService(HttpClient httpClient, IRepository<SmsLog> repository)
        {
            _httpClient = httpClient;
            _repository = repository;
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message,long? userId, long? creatorUserId, CancellationToken ct)
        {
            var requestBody = new
            {
                apiKey = "",
                mobile = phoneNumber,
                message = message,
            };

            var model = new SmsLog
            {
                Text = message,
                Mobile = phoneNumber,
                ReceiverUserId = userId,
                CreatorUserId = creatorUserId,
            };
            await _repository.AddAsync(model, ct);

            return true;
            var response = await _httpClient.PostAsJsonAsync("https://api.kavenegar.com/v1/your-api-key/sms/send.json", requestBody);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
    }
}
