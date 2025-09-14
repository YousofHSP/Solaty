using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Common.Utilities;
using Data.Contracts;
using Domain.Entities;
using Domain.Message;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Message;
using Shared;
using Shared.DTOs;
using WebFramework.Api;

namespace Api.Controllers.v1
{

    [Display(Name = "پیامک")]
    [ApiVersion("1")]
    public class SmsController : BaseController
    {
        private readonly IRepository<SmsLog> _repository;
        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;

        public SmsController(IRepository<SmsLog> repository, IMapper mapper, IMessageService messageService)
        {
            _repository = repository;
            _mapper = mapper;
            _messageService = messageService;
        }

        protected IQueryable<SmsLog> setSearch(string? search, IQueryable<SmsLog> query)
        {
            if (string.IsNullOrEmpty(search)) return query;
            query = query.Where(i => i.CreatorUser.UserName.Contains(search)
                || i.Mobile.Contains(search)
                || i.ReceiverUser.UserName.Contains(search));
            return query;
        }

        [HttpPost("[action]")]
        [Display(Name = "لیست")]
        public async Task<ApiResult<IndexResDto<SmsLogResDto>>> Index(IndexDto dto, CancellationToken ct)
        {
            dto.Page = Math.Max(dto.Page, 1);
            dto.Limit = Math.Max(dto.Limit, 10);

            var query = _repository.TableNoTracking.AsQueryable();
            query = query.Include(i => i.CreatorUser).Include(i => i.ReceiverUser);
            query = setSearch(dto.Search, query);

            if (dto.Sort is null || string.IsNullOrEmpty(dto.Sort.By) || string.IsNullOrEmpty(dto.Sort.Type))
            {
                query = query.OrderByDescending(i => i.Id);
            }
            else
            {
                query = setSort(dto.Sort, query);
            }

            var total = await query.CountAsync(ct);
            var models = await query
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<SmsLogResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(new IndexResDto<SmsLogResDto>
            {
                Data = models,
                Page = dto.Page,
                Limit = dto.Limit,
                Total = total
            });
        }

        [HttpPost("[action]")]
        [Display(Name = "ارسال مجدد")]
        public async Task<ApiResult> ReSend([FromBody] ReSendDto dto, CancellationToken ct)
        {
            var smsLog = await _repository.TableNoTracking
                .FirstOrDefaultAsync(i => i.Id == dto.Id);
            if (smsLog is null)
                throw new NotFoundException("پیدا نشد");

            var creatorUserId = User.Identity?.GetUserId<int>();
            await _messageService.SendMessageAsync(smsLog.Mobile, smsLog.Text, smsLog.ReceiverUserId, creatorUserId, ct);

            return Ok();
        }
    }
}
