using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using Common.Exceptions;
using Common.Utilities;
using Data.Contracts;
using Domain.Entities;
using Domain.Entities.Common;
using Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DTOs;

namespace WebFramework.Api;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:ApiVersion}/[controller]")]
public class CrudController<TDto, TResDto, TEntity, TKey>(
    IRepository<TEntity> repository,
    IMapper mapper)
    : BaseController<TKey>
    where TEntity : class, IEntity<TKey>
    where TDto : BaseDto<TDto, TEntity, TKey>
    where TResDto : BaseDto<TResDto, TEntity, TKey>
{
    protected readonly IRepository<TEntity> Repository = repository;
    protected readonly IMapper Mapper = mapper;

    virtual protected async Task<TEntity> setProperty(TEntity entity)
    {
        return await Task.FromResult(entity);
    }

    virtual protected IQueryable<TEntity> setSearch(string? search, IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> setIncludes(IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual async Task checkPermission(HttpContext context, string? permission = null)
    {
        var userId = User.Identity!.GetUserId<int>();
        if (userId == 1)
            return;
        if (permission is null)
        {
            var controller = context.GetRouteData().Values["controller"]?.ToString();
            var action = context.GetRouteData().Values["action"]?.ToString();
            permission = $"{controller}.{action}";
        }

        // var roles = await roleRepository.TableNoTracking
        //     .Where(i => i.UserGroups.Any(g => g.Users.Any(u => u.Id == userId)))
        //     .ToListAsync();
        // var permissions = new List<string>();
        // foreach (var role in roles)
        // {
        //     var claims = await roleManager.GetClaimsAsync(role);
        //     permissions.AddRange(claims.Select(i => i.Value));
        // }
        //
        // if (!permissions.Any(i => i == permission))
        //     throw new AppException(ApiResultStatusCode.UnAuthorized, "دسترسی ندارید");
    }

    protected virtual IQueryable<TEntity> setSort(IndexSortDto? sort, IQueryable<TEntity> query)
    {
        if (sort is null || string.IsNullOrEmpty(sort.By) || string.IsNullOrEmpty(sort.Type))
        {
            query = query.OrderByDescending(i => i.Id);
        }
        else
        {
            query = setSort<TEntity>(sort, query);
        }

        return query;
    }

    [Display(Name = "لیست")]
    [HttpPost("[action]")]
    public virtual async Task<ApiResult<IndexResDto<TResDto>>> Index([FromBody] IndexDto dto,
        CancellationToken cancellationToken)
    {
        await checkPermission(HttpContext);
        dto.Page = Math.Max(dto.Page, 1);
        dto.Limit = Math.Max(dto.Limit, 10);
        var query = Repository.TableNoTracking.AsQueryable();
        query = setFilters(dto.Filters, query);
        query = setIncludes(query);
        query = setSearch(dto.Search, query);
        query = setSort(dto.Sort, query);

        var total = await query.CountAsync(cancellationToken);
        query = query
            .Skip((dto.Page - 1) * dto.Limit)
            .Take(dto.Limit);

        var list = await query
            .ProjectTo<TResDto>(Mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Ok(new IndexResDto<TResDto>
        {
            Data = list,
            Total = total,
            Page = dto.Page,
            Limit = dto.Limit
        });
    }

    [Display(Name = "اطلاعات")]
    [HttpGet("{id}")]
    public virtual async Task<ApiResult<TResDto>> Get(TKey id, CancellationToken cancellationToken)
    {
        var query = Repository.TableNoTracking.AsQueryable();
        query = setIncludes(query);
        var model = await query
            .SingleOrDefaultAsync(p => p.Id!.Equals(id), cancellationToken);
        if (model == null)
            return NotFound();
        var dto = Mapper.Map<TResDto>(model);
        return dto;
    }

    [Display(Name = "ایجاد")]
    [HttpPost("[action]")]
    public virtual async Task<ApiResult<TResDto>> Create(TDto dto, CancellationToken cancellationToken)
    {
        await checkPermission(HttpContext);
        var model = dto.ToEntity(Mapper);
        if (model is BaseEntity entity)
            entity.CreatorUserId = User.Identity!.GetUserId<int>();

        model = await setProperty(model);
        await Repository.AddAsync(model, cancellationToken);
        var resultDto = await Repository.TableNoTracking.ProjectTo<TResDto>(Mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(p => p.Id!.Equals(model.Id), cancellationToken);
        return resultDto!;
    }

    [Display(Name = "ویرایش")]
    [HttpPatch("[action]")]
    public virtual async Task<ApiResult<TResDto>> Update(TDto dto, CancellationToken cancellationToken)
    {
        await checkPermission(HttpContext);
        var model = await Repository.GetByIdAsync(cancellationToken, dto.Id);
        if (model is null)
            throw new NotFoundException("پیدا نشد");

        model = dto.ToEntity(model, Mapper);
        model = await setProperty(model);

        await Repository.UpdateAsync(model, cancellationToken);
        var resultDto = await Repository.TableNoTracking
            .ProjectTo<TResDto>(Mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(p => p.Id!.Equals(model.Id), cancellationToken);
        return resultDto!;
    }

    [Display(Name = "حذف")]
    [HttpDelete("{id}")]
    public virtual async Task<ApiResult> Delete(TKey id, CancellationToken cancellationToken)
    {
        await checkPermission(HttpContext);
        var model = await Repository.GetByIdAsync(cancellationToken, id!);
        if (model is null) throw new NotFoundException();

        await Repository.DeleteAsync(model, cancellationToken);

        return Ok();
    }

    [Display(Name = "لیست انتخابی")]
    [HttpGet("[action]")]
    public virtual async Task<ApiResult<List<SelectDto>>> GetSelectList(CancellationToken cancellationToken)
    {
        var models = await Repository.TableNoTracking.ToListAsync(cancellationToken);
        var list = models.Select(i =>
        {
            var id = i.Id;
            string? title = "";

            var titleProp = i.GetType().GetProperty("Title");
            if (titleProp != null)
            {
                var value = titleProp.GetValue(i);
                title = value?.ToString() ?? "";
            }

            return new SelectDto { Id = id, Title = title };
        }).ToList();
        return Ok(list);
    }
}

public class CrudController<TDto, TResDto, TEntity>(
    IRepository<TEntity> repository,
    IMapper mapper)
    : CrudController<TDto, TResDto, TEntity, long>(repository, mapper)
    where TEntity : class, IEntity<long>
    where TDto : BaseDto<TDto, TEntity, long>
    where TResDto : BaseDto<TResDto, TEntity, long>;

public class CrudController<TDto, TEntity>(
    IRepository<TEntity> repository,
    IMapper mapper)
    : CrudController<TDto, TDto, TEntity, long>(repository, mapper)
    where TEntity : class, IEntity<long>
    where TDto : BaseDto<TDto, TEntity, long>;