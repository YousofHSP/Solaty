using Asp.Versioning;
using AutoMapper;
using Common;
using Common.Exceptions;
using Common.Utilities;
using Data.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Model;
using Shared;
using Shared.DTOs;
using WebFramework.Api;

namespace Api.Controllers.v1;

[ApiVersion("1")]
public class ImportedRecordController : BaseController
{
    private readonly IRepository<ImportedRecord> _repository;
    private readonly IMapper _mapper;
    private readonly IRepository<ImportedFile> _importedFileRepository;

    public ImportedRecordController(IRepository<ImportedRecord> repository, IMapper mapper,
         IRepository<ImportedFile> importedFileRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _importedFileRepository = importedFileRepository;
    }

    [HttpPost("[action]")]
    public async Task<ApiResult<IndexResDto<ImportedRecordResDto>>> Index(ImportedRecordFilterDto dto,
        CancellationToken ct)
    {
        dto.Page = Math.Max(dto.Page, 1);
        dto.Limit = Math.Max(dto.Limit, 10);
        var query = _repository.TableNoTracking.AsQueryable();
        if (dto.Status is not null)
            query = query.Where(i => i.Status == dto.Status);
        if (dto.Type is not null)
            query = query.Where(i => i.ImportedFile.Type == dto.Type);
        if (!string.IsNullOrEmpty(dto.Date))
        {
            var date = dto.Date.ToMiladi();
            query = query.Where(i => i.CreateDate.Date == date.Date);
        }

        if (!string.IsNullOrEmpty(dto.Search))
        {
            var search = dto.Search.Fa2En().FixPersianChars();
            query = query.Where(i => i.Title.Contains(search) || i.Data.Contains(search));
        }

        var total = await query.CountAsync(ct);
        var models = await query
            .Skip((dto.Page - 1) * dto.Limit)
            .Take(dto.Limit)
            .Include(i => i.ImportedFile)
            .ToListAsync(ct);
        var list = _mapper.Map<List<ImportedRecordResDto>>(models);

        return Ok(new IndexResDto<ImportedRecordResDto>
        {
            Data = list,
            Total = total,
            Page = dto.Page,
            Limit = dto.Limit
        });
    }

    [HttpPost("[action]/{id:long}")]
    public async Task<ApiResult> Adapt([FromRoute] long id, AdaptImportedRecordDto dto, CancellationToken ct)
    {
        var model = await _repository.TableNoTracking
            .Include(i => i.ImportedFile)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
        if (model is null)
            throw new NotFoundException("رکورد یافت نشد");

        model.Status = ImportedRecordStatus.Adapted;
        await _repository.UpdateAsync(model, ct);
        return Ok();
    }

    [HttpGet("[action]/{id:guid}")]
    public async Task<ApiResult> Ignore([FromRoute] Guid id, CancellationToken ct)
    {
        var model = await _repository.GetByIdAsync(ct, id);
        if (model is null)
            throw new NotFoundException("رکورد پیدا نشد");
        model.Status = ImportedRecordStatus.Ignored;
        await _repository.UpdateAsync(model, ct);
        return Ok();
    }

    [HttpGet("[action]")]
    public async Task<List<SelectDto>> GetFileSelectList(CancellationToken ct)
    {
        return await _importedFileRepository.TableNoTracking
            .OrderByDescending(i => i.Id)
            .Select(i => new SelectDto(i.Id, i.Type.ToDisplay(EnumExtensions.DisplayProperty.Name) + "-" + i.Title))
            .ToListAsync(ct);
    }
}