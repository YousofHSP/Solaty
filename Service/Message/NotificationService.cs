using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Data.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Service.Message
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _repository;
        private readonly IRepository<ApiToken> _apiTokenRepository;
        private IMapper _mapper;

        public NotificationService(IRepository<Notification> repository,  IMapper mapper, IRepository<ApiToken> apiTokenRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _apiTokenRepository = apiTokenRepository;
        }

        public async Task SendNotification(int userId, string title, int creatorUserId, CancellationToken ct)
        {
            var model = new Notification
            {
                UserId = userId,
                Title = title,
                Status = NotificationStatus.Unseen
            };
            await _repository.AddAsync(model, ct);
            var count  = await _repository.TableNoTracking.CountAsync(i => i.UserId == userId && i.Status == NotificationStatus.Unseen, ct);
            var dto = NotificationResDto.FromEntity(model, _mapper);
            var apiTokens = await _apiTokenRepository.TableNoTracking
                .Where(i => i.UserId == userId && i.Enable == true)
                .ToListAsync(ct);

            // await _appHubContext.Clients.All.SendAsync("ReceiveNotification", dto, count, apiTokens.Select(i => i.Code), ct);

        }
        public async Task SendNotifications(string apiTokenCode, CancellationToken ct)
        {
            var apiToken = await _apiTokenRepository.TableNoTracking
                .FirstOrDefaultAsync(i => i.Code == apiTokenCode && i.Enable == true, ct);
            if (apiToken is null)
                throw new AppException(Common.ApiResultStatusCode.UnAuthorized, "سشن معتبر نیست");
            var list = await _repository.TableNoTracking
                .Where(i => i.UserId == apiToken.UserId)
                .Where(i => i.Status == NotificationStatus.Unseen)
                .ProjectTo<NotificationResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            // await _appHubContext.Clients.All.SendAsync("ReceiveNotifications", list, list.Count, apiTokenCode, ct);
        }
    }
}
