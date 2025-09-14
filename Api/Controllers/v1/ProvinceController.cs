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
    [Display(Name = "استان")]
    [ApiVersion("1")]
    public class ProvinceController : BaseController
    {
        private readonly IRepository<Province> _repository;
        private readonly IMapper _mapper;

        public ProvinceController(IRepository<Province> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [Display(Name = "لیست")]
        [HttpPost("[action]")]
        public async Task<ApiResult<IndexResDto<ProvinceResDto>>> Index(IndexDto dto, CancellationToken ct)
        {
            dto.Page = Math.Max(dto.Page, 1);
            dto.Limit = Math.Max(dto.Limit, 10);
            var query = _repository.TableNoTracking.AsQueryable();
            query = setFilters<Province>(dto.Filters, query);
            var total = await query.CountAsync(ct);
            var list = await query
                .OrderByDescending(i => i.Id)
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<ProvinceResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(new IndexResDto<ProvinceResDto>
            {
                Data = list,
                Total = total,
                Page = dto.Page,
                Limit = dto.Limit
            });

        }

        [Display(Name = "ویرایش")]
        [HttpPatch("[action]")]
        public async Task<ApiResult<ProvinceResDto>> Update(ProvinceDto dto, CancellationToken ct)
        {

            var model = await _repository.GetByIdAsync(ct, dto.Id);
            if (model is null) throw new NotFoundException("استان پیدا نشد");
            model = dto.ToEntity(model, _mapper);

            await _repository.UpdateAsync(model, ct);

            return ProvinceResDto.FromEntity(model, _mapper);
        }

    }
}
