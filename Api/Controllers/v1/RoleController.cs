using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using Common;
using Common.Exceptions;
using Data.Contracts;
using Domain.Entities;
using Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DTOs;
using WebFramework.Api;

namespace Api.Controllers.v1
{
    [ApiVersion("1")]
    [Display(Name = "نقش")]
    public class RoleController : CrudController<RoleDto, RoleResDto, Role>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IMapper _mapper;
        protected override IQueryable<Role> setSearch(string? search, IQueryable<Role> query)
        {
            query = query.Where(i => i.Name != "Admin");
            if (string.IsNullOrWhiteSpace(search)) return query;
            query = query.Where(i => i.Title.Contains(search) || i.Name.Contains(search));
            return query;
        }
        public override async Task<ApiResult<RoleResDto>> Get(long id, CancellationToken cancellationToken)
        {
            var model = await _roleManager.FindByIdAsync(id.ToString());
            if (model is null)
                throw new NotFoundException("نقش پیدا نشد");
            var dto = RoleResDto.FromEntity(model, _mapper);
            var claims = await _roleManager.GetClaimsAsync(model);
            dto.Permissions = claims.Select(i => i.Value).ToList();
            return Ok(dto);
        }
        public RoleController(IRepository<Role> repository, IMapper mapper, RoleManager<Role> roleManager)
            : base(repository, mapper)
        {
            _mapper = mapper;
            _roleManager = roleManager;
            _roleRepository = repository;
        }

        [HttpPost("[action]")]
        [Display(Name = "ایجاد")]
        public override async Task<ApiResult<RoleResDto>> Create(RoleDto dto, CancellationToken cancellationToken)
        {
            var model = dto.ToEntity(_mapper);
            model.NormalizedName = _roleManager.NormalizeKey(model.Name);
            var checkName = await _roleRepository.TableNoTracking
                .AnyAsync(r => r.NormalizedName == model.NormalizedName);
            if (checkName)
                throw new AppException(Common.ApiResultStatusCode.BadRequest, "نام نقش نباید تکراری باشد");
            if (!Permissions.AreValidPermissions(dto.Permissions))
                throw new BadRequestException("دسترسی ها معتبر نیستند");
            await _roleRepository.AddAsync(model, cancellationToken);
            foreach (var per in dto.Permissions)
                await _roleManager.AddClaimAsync(model, new System.Security.Claims.Claim("Permission", per));
            var resDto = RoleResDto.FromEntity(model, _mapper);
            return Ok(resDto);
        }

        [Display(Name = "ویرایش")]
        public override async Task<ApiResult<RoleResDto>> Update(RoleDto dto, CancellationToken cancellationToken)
        {
            var model = await _roleRepository.GetByIdAsync(cancellationToken, dto.Id);
            if (model is null)
                throw new NotFoundException("نقش پیدا نشد");
            model = dto.ToEntity(model, _mapper);
            model.NormalizedName = _roleManager.NormalizeKey(model.Name);
            var checkName = await _roleRepository.TableNoTracking
                .AnyAsync(r => r.NormalizedName == model.NormalizedName && r.Id != model.Id, cancellationToken);
            if (checkName)
                throw new AppException(Common.ApiResultStatusCode.BadRequest, "نام نقش نباید تکراری باشد");

            // if (!Permissions.AreValidPermissions(dto.Permissions))
            //     throw new BadRequestException("دسترسی ها معتبر نیستند");
            await _roleRepository.UpdateAsync(model, cancellationToken);
            var currentPermissions = await _roleManager.GetClaimsAsync(model);
            foreach (var permission in currentPermissions)
                await _roleManager.RemoveClaimAsync(model, permission);

            foreach (var per in dto.Permissions)
                await _roleManager.AddClaimAsync(model, new System.Security.Claims.Claim("Permission", per));
            var resDto = RoleResDto.FromEntity(model, _mapper);
            return Ok(resDto);
        }

        [Display(Name = "حذف")]
        public override async Task<ApiResult> Delete(long id, CancellationToken cancellationToken)
        {
            var model = await _roleRepository.GetByIdAsync(cancellationToken, id);
            if (model is null)
                throw new NotFoundException("نقش پیدا نشد");
            await _roleRepository.DeleteAsync(model, cancellationToken);
            return Ok();
        }
        [HttpGet("[action]")]
        [Display(Name = "تمام دسترسی ها")]
        public ApiResult<List<Permission>> AllPermissions()
        {
            var permissions = Permissions.All;

            return Ok(permissions);
        }

        public override async Task<ApiResult<List<SelectDto>>> GetSelectList(CancellationToken cancellationToken)
        {
            var list = await _roleRepository.TableNoTracking
                .Where(i => i.Name != "Admin")
                .ToListAsync(cancellationToken);
            return Ok(list.Select(i => new SelectDto { Id = i.Id, Title = i.Title}).ToList());
        }

    }
}
