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
        private readonly IRepository<Audit> _auditRepository;

        private readonly IRepository<Log> _logRepository;
        private readonly IRepository<ArchiveLog> _archiveLogRepository;
        private readonly IMapper _mapper;
        public LogController(IRepository<Audit> auditRepository, IRepository<Log> logRepository, IRepository<ArchiveLog> archiveLogRepository, IMapper mapper)
        {
            _auditRepository = auditRepository;
            _logRepository = logRepository;
            _archiveLogRepository = archiveLogRepository;
            _mapper = mapper;
        }


        protected IQueryable<Audit> setSearch(string? search, IQueryable<Audit> query)
        {
            if (search is null)
                return query;
            query = query.Where(i =>
                i.Ip.Contains(search)
                || i.ReferrerLink.Contains(search)
                || i.Protocol.Contains(search)
                || i.PhysicalPath.Contains(search)
                || i.RequestId.Contains(search)
                || i.Model.Contains(search)
                || i.Method.Contains(search)
                || i.UserAgent.Contains(search)
                );
            return query;
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
        [HttpPost("[action]")]
        [Display(Name = "لیست ممیزی ها")]
        public async Task<ApiResult<IndexResDto<AuditDto>>> AuditIndex(IndexDto dto, CancellationToken ct)
        {
            dto.Page = Math.Max(dto.Page, 1);
            dto.Limit = Math.Max(dto.Limit, 10);

            var query = _auditRepository.TableNoTracking.AsQueryable();
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
                .ProjectTo<AuditDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(new IndexResDto<AuditDto>
            {
                Data = models,
                Page = dto.Page,
                Limit = dto.Limit,
                Total = total
            });
        }

        [HttpPost("[action]")]
        [Display(Name = "ثبت آرشیو")]
        public async Task<ApiResult<object>> ArchiveLogs(ArchiveRequestDto dto, CancellationToken ct)
        {
            //if (dto.UntilDate > DateTime.UtcNow.AddMonths(-1))
            //    return BadRequest("تاریخ باید حداقل ۱ ماه قبل از امروز باشد.");

            var archiveFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "log-archives");
            Directory.CreateDirectory(archiveFolder);

            var archiveFileName = $"logs_audits_archive_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            var archivePath = Path.Combine(archiveFolder, archiveFileName);


            var parts = dto.UntilDate.Split('/');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            var pc = new System.Globalization.PersianCalendar();
            var untilDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
            // جمع‌آوری لاگ‌ها
            var logs = await _logRepository.Table
                .Where(x => x.TimeStamp <= untilDate)
            .OrderBy(x => x.TimeStamp)
                .ToListAsync();

            var audits = await _auditRepository.Table
                .Where(x => x.CreateDate <= untilDate)
                .OrderBy(x => x.CreateDate)
                .ToListAsync();

            if (!logs.Any() && !audits.Any())
                return BadRequest("لاگ یا آدیتی برای آرشیو وجود ندارد.");

            // ذخیره موقت فایل‌ها
            var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            Directory.CreateDirectory(tempFolder);

            var logsFile = Path.Combine(tempFolder, $"logs_{Guid.NewGuid()}.json");
            var auditsFile = Path.Combine(tempFolder, $"audits_{Guid.NewGuid()}.json");

            await System.IO.File.WriteAllTextAsync(logsFile, JsonSerializer.Serialize(logs));
            await System.IO.File.WriteAllTextAsync(auditsFile, JsonSerializer.Serialize(audits));

            // فشرده‌سازی
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(logsFile, "logs.json");
                archive.CreateEntryFromFile(auditsFile, "audits.json");
            }

            // حذف رکوردهای آرشیوشده
            //await _logRepository.DeleteRangeAsync(logs, ct);
            //await _auditRepository.DeleteRangeAsync(audits, ct);

            // ثبت گزارش آرشیو
            var archiveLog = new ArchiveLog
            {
                ArchiveFileName = archiveFileName,
                ArchivedUntilDate = untilDate,
                ArchivedAt = DateTimeOffset.Now,
                LogCount = logs.Count,
                AuditCount = audits.Count,
                CreatorUserId = User.Identity?.GetUserId<int>() ?? 0
            };
            await _archiveLogRepository.AddAsync(archiveLog, ct);

            // پاک کردن فایل موقت
            System.IO.File.Delete(logsFile);
            System.IO.File.Delete(auditsFile);

            return Ok(new
            {
                Message = "آرشیو با موفقیت انجام شد.",
                ArchiveFileName = archiveFileName
            });
        }
        [HttpPost("[action]")]
        [Display(Name = "لیست آرشیوها")]
        public async Task<ApiResult<IndexResDto<ArchiveLogResDto>>> ArchiveLogsIndex(IndexDto dto, CancellationToken ct)
        {


            dto.Page = Math.Max(dto.Page, 1);
            dto.Limit = Math.Max(dto.Limit, 10);

            var total = await _archiveLogRepository.TableNoTracking.CountAsync(ct);
            var models = await _archiveLogRepository.TableNoTracking
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<ArchiveLogResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Ok(new IndexResDto<ArchiveLogResDto>
            {
                Data = models,
                Page = dto.Page,
                Limit = dto.Limit,
                Total = total
            });
        }
        [HttpPost("[action]")]
        [Display(Name = "بازگردانی آرشیو")]
        public async Task<IActionResult> RestoreArchive(int archiveLogId, CancellationToken ct)
        {
            var archiveLog = await _archiveLogRepository.GetByIdAsync(ct, archiveLogId);
            if (archiveLog is null)
                throw new NotFoundException("آرشیو پیدا نشد");
            var archiveFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "log-archives");
            var archivePath = Path.Combine(archiveFolder, archiveLog.ArchiveFileName);

            if (!System.IO.File.Exists(archivePath))
                throw new NotFoundException("آرشیو پیدا نشد");

            var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "TempRestore");
            Directory.CreateDirectory(tempFolder);

            ZipFile.ExtractToDirectory(archivePath, tempFolder, true);

            var logsFile = Path.Combine(tempFolder, "logs.json");
            var auditsFile = Path.Combine(tempFolder, "audits.json");

            if (System.IO.File.Exists(logsFile))
            {
                var logsJson = await System.IO.File.ReadAllTextAsync(logsFile);
                var logs = JsonSerializer.Deserialize<List<Log>>(logsJson);
                if (logs?.Any() == true)
                {
                    await _logRepository.AddRangeAsync(logs.Select(i => { i.Id = 0; return i; }), ct);
                }
            }

            if (System.IO.File.Exists(auditsFile))
            {
                var auditsJson = await System.IO.File.ReadAllTextAsync(auditsFile);
                var audits = JsonSerializer.Deserialize<List<Audit>>(auditsJson);
                if (audits?.Any() == true)
                {
                    await _auditRepository.AddRangeAsync(audits.Select(i => { i.Id = 0; return i; }), ct);
                }
            }

            Directory.Delete(tempFolder, true);

            return Ok(new { Message = "آرشیو با موفقیت بازگردانی شد." });
        }
    }
}
