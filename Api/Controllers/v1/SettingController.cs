using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
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
    [Display(Name = "تنظیمات")]
    public class SettingController : BaseController
    {
        private readonly ISettingService _settingService;
        private readonly IRepository<Setting> _repository;
        private readonly IMapper _mapper;
        public SettingController(IRepository<Setting> repository, IMapper mapper, ISettingService settingService)         {
            _settingService = settingService;
            _repository = repository;   
            _mapper = mapper;
        }

        [Display(Name = "لیست")]
        [HttpPost("[action]")]
        public async Task<ApiResult<IndexResDto<SettingResDto>>> Index(IndexDto dto, CancellationToken ct)
        {

            dto.Page = Math.Max(dto.Page, 1);
            dto.Limit = Math.Max(dto.Limit, 10);
            var query = _repository.TableNoTracking.AsQueryable();
            query = setFilters<Setting>(dto.Filters, query);
            var total = await query.CountAsync(ct);
            var list = await query
                .OrderByDescending(i => i.Id)
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<SettingResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(new IndexResDto<SettingResDto>
            {
                Data = list,
                Total = total,
                Page = dto.Page,
                Limit = dto.Limit
            });
        }

        
        [Display(Name = "لیست")]
        [HttpPost("[action]")]
        public async Task<ApiResult<SettingResDto>> Update(SettingDto dto, CancellationToken ct)
        {
            var model = await _repository.GetByIdAsync(ct, dto.Id);
            if (model is null)
                throw new NotFoundException("پیدا نشد");
            model = dto.ToEntity(model, _mapper);

            await _repository.UpdateAsync(model, ct);
            var result = SettingResDto.FromEntity(model, _mapper);
            return Ok(result);
        }
    }
}
