using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
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
    [Display(Name = "شهر")]
    [ApiVersion("1")]
    public class CityController : BaseController
    {
        private readonly IRepository<City> _repository;
        private readonly IRepository<ApiToken> _apiTokenRepository;
        private readonly IMapper _mapper;

        public CityController(IMapper mapper, IRepository<City> repository, IRepository<ApiToken> apiTokenRepository)
        {
            _mapper = mapper;
            _repository = repository;
            _apiTokenRepository = apiTokenRepository;
        }
        [Display(Name = "لیست")]
        [HttpPost("[action]")]
        public async Task<ApiResult<IndexResDto<CityResDto>>> Index(IndexDto dto, CancellationToken ct)
        {
            dto.Page = Math.Max(dto.Page, 1);
            dto.Limit = Math.Max(dto.Limit, 10);
            var query = _repository.TableNoTracking.AsQueryable();
            query = setFilters<City>(dto.Filters, query);
            var total = await query.CountAsync(ct);
            var list = await query
                .OrderByDescending(i => i.Id)
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .Include(i => i.Province)
                .ProjectTo<CityResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(new IndexResDto<CityResDto>
            {
                Data = list,
                Total = total,
                Page = dto.Page,
                Limit = dto.Limit
            });

        }

        [Display(Name = "ویرایش")]
        [HttpPatch("[action]")]
        public async Task<ApiResult<CityResDto>> Update(CityDto dto, CancellationToken ct)
        {

            var model = await _repository.GetByIdAsync(ct, dto.Id);
            if (model is null) throw new NotFoundException("استان پیدا نشد");
            model = dto.ToEntity(model, _mapper);

            await _repository.UpdateAsync(model, ct);

            return CityResDto.FromEntity(model, _mapper);
        }
    }
}
