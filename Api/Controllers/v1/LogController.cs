using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text.Json;
using Asp.Versioning;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Common.Utilities;
using Data.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DTOs;
using WebFramework.Api;

namespace Api.Controllers.v1
{
    [ApiVersion("1")]
    [Display(Name = "لاگ و ممیزی")]
    public class LogController : BaseController
    {

        private readonly IRepository<Log> _logRepository;
        private readonly IMapper _mapper;
        public LogController( IRepository<Log> logRepository, IMapper mapper)
        {
            _logRepository = logRepository;
            _mapper = mapper;
        }


        protected IQueryable<Log> setSearch(string? search, IQueryable<Log> query)
        {
            if (search is null)
                return query;
            query = query.Where(i =>
                i.Level.Contains(search)
                || i.Message.Contains(search)
                || i.Exception.Contains(search)
                || i.CallSite.Contains(search)
                || i.UserName.Contains(search)
                || i.IpAddress.Contains(search)
                || i.RequestId.Contains(search)
                || i.UserAgent.Contains(search)
                );
            return query;
        }
        [HttpPost("[action]")]
        [Display(Name = "لیست لاگ ها")]
        public async Task<ApiResult<IndexResDto<LogResDto>>> LogIndex(IndexDto dto, CancellationToken ct)
        {
            dto.Page = Math.Max(dto.Page, 1);
            dto.Limit = Math.Max(dto.Limit, 10);

            var query = _logRepository.TableNoTracking.AsQueryable();
            query = setSearch(dto.Search, query);
            if (dto.Sort is null || string.IsNullOrEmpty(dto.Sort.By) || string.IsNullOrEmpty(dto.Sort.Type))
            {
                query = query.OrderByDescending(i => i.Id);
            }
            else
            {
                query = setSort(dto.Sort, query);
            }

            var total = await query.CountAsync();
            var models = await query
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<LogResDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(new IndexResDto<LogResDto>
            {
                Data = models,
                Page = dto.Page,
                Limit = dto.Limit,
                Total = total
            });
        }
    }
}
