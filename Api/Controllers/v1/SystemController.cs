using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using Asp.Versioning;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using Common.Exceptions;
using Common.Utilities;
using Data.Contracts;
using Domain.Entities;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Service.License;
using Shared;
using Shared.DTOs;
using WebFramework.Api;

namespace Api.Controllers.v1
{
    [ApiVersion("1")]
    [Display(Name = "سیستم")]
    public class SystemController : BaseController
    {
        private readonly string _connectionString;
        private readonly IRepository<Backup> _repository;
        private readonly IRepository<AuditCheck> _auditCheckRepository;
        private readonly IRepository<AuditCheckDetail> _auditCheckDetailRepository;
        private readonly IHashEntityValidator _hashEntityValidator;
        private readonly ILicenseService _licenseService;
        private readonly IMapper _mapper;

        public SystemController(IConfiguration configuration, IRepository<Backup> repository, IMapper mapper, IHashEntityValidator hashEntityValidator, IRepository<AuditCheck> auditCheckRepository, IRepository<AuditCheckDetail> auditCheckDetailRepository, ILicenseService licenseService)
        {
            _connectionString = configuration.GetConnectionString("SqlServer") ?? "";
            _repository = repository;
            _mapper = mapper;
            _hashEntityValidator = hashEntityValidator;
            _auditCheckRepository = auditCheckRepository;
            _auditCheckDetailRepository = auditCheckDetailRepository;
            _licenseService = licenseService;
        }
        [Display(Name = "بکآپ دیتابیس")]
        [HttpGet("BackupDatabase")]
        public async Task<ApiResult> BackupDatabase(CancellationToken ct)
        {
            var backupFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups");
            Directory.CreateDirectory(backupFolderPath); // اگه وجود نداشت، می‌سازه

            var backupFileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var backupZipFileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            var backupFilePath = Path.Combine(backupFolderPath, backupFileName);
            var backupZipFilePath = Path.Combine(backupFolderPath, backupZipFileName); 

            var databaseName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;

            var sqlBackupQuery = $@"BACKUP DATABASE [{databaseName}]
                                TO DISK = '{backupFilePath}'
                                WITH FORMAT,
                                     MEDIANAME = 'DbBackups',
                                     NAME = 'Full Backup of 
            {databaseName}';";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(sqlBackupQuery, connection))
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
                
                var backupFile = Path.Combine(backupFolderPath, backupFileName);

                using (var archive = ZipFile.Open(backupZipFilePath, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(backupFile, "backup.bak");
                }

                var downloadUrl = $"{Request.Scheme}://{Request.Host}/api/DatabaseBackup/download/{backupFileName}";
                var model = new Backup
                {
                    CreatorUserId = User.Identity?.GetUserId<int>() ?? 0,
                    FileName = backupZipFileName
                };

                System.IO.File.Delete(backupFile);
                await _repository.AddAsync(model, ct);


                return Ok();
            }
            catch (Exception ex)
            {
                throw new AppException(Common.ApiResultStatusCode.ServerError, $"خطا در بکاپ گرفتن: {ex.Message}");
            }
        }

        protected IQueryable<Backup> setSearch(string? search, IQueryable<Backup> query)
        {
            if (search is null)
                return query;
            query = query.Where(i =>
                i.FileName.Contains(search)
                || i.CreatorUser.UserName.Contains(search)
                );
            return query;
        }
        [Display(Name = "لیست بکآپ ها")]
        [HttpPost("[action]")]
        public async Task<IndexResDto<BackupDto>> BackupsList(IndexDto dto, CancellationToken ct)
        {
            dto.Page = Math.Max(1, dto.Page);
            dto.Limit = Math.Max(10, dto.Limit);
            var query = _repository.TableNoTracking.AsQueryable();
            query.Include(i => i.CreatorUser);
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
            var list = await query
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<BackupDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            var result = new IndexResDto<BackupDto>()
            {
                Data = list,
                Limit = dto.Limit,
                Page = dto.Page,
                Total = total
            };
            return result;
        }
        [Display(Name = "دانلود بکآپ")]
        [Produces("application/octet-stream")]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadBackup(int id, CancellationToken ct)
        {
            var backup = await _repository.TableNoTracking.FirstAsync(i => i.Id == id);
            if (backup is null)
                throw new NotFoundException("فایل پیدا نشد");
            var fileName = backup.FileName;
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("..") || Path.GetExtension(fileName).ToLower() != ".zip")
            {
                throw new BadRequestException("اسم فایل معتبر نیست");
            }

            var backupFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups");
            var filePath = Path.Combine(backupFolderPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                throw new NotFoundException("فایل پیدا نشد.");
            }

            var contentType = "application/octet-stream"; // فایل باینری
            return PhysicalFile(filePath, contentType, fileName);
        }

        protected IQueryable<AuditCheck> setSearch(string? search, IQueryable<AuditCheck> query)
        {

            if (string.IsNullOrEmpty(search)) return query;
            query = query.Where(i => i.CreatorUser.UserName.Contains(search));
            return query;
        }
        protected IQueryable<AuditCheckDetail> setSearch(string? search, IQueryable<AuditCheckDetail> query)
        {

            if (string.IsNullOrEmpty(search)) return query;
            query = query.Where(i => i.Model.Contains(search));
            return query;
        }
        [Display(Name = "لیست بررسی جداول")]
        [HttpPost("[action]")]
        public async Task<ApiResult<IndexResDto<AuditCheckResDto>>> AuditsCheckIndex(IndexDto dto, CancellationToken ct)
        {
            
            dto.Page = Math.Max(1, dto.Page);
            dto.Limit = Math.Max(10, dto.Limit);
            var query = _auditCheckRepository.TableNoTracking.AsQueryable();
            query.Include(i => i.CreatorUser);
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
            var list = await query
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<AuditCheckResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            var result = new IndexResDto<AuditCheckResDto>()
            {
                Data = list,
                Limit = dto.Limit,
                Page = dto.Page,
                Total = total
            };
            return result;
        }

        [Display(Name = "جزيیات لیست بررسی جداول")]
        [HttpPost("[action]/{auditCheckId}")]
        public async Task<ApiResult<IndexResDto<AuditCheckDetailResDto>>> AuditsCheckDetailIndex(IndexDto dto, [FromRoute] int auditCheckId, CancellationToken ct)
        {
            
            dto.Page = Math.Max(1, dto.Page);
            dto.Limit = Math.Max(10, dto.Limit);
            var query = _auditCheckDetailRepository.TableNoTracking.AsQueryable();
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
            var list = await query
                .Skip((dto.Page - 1) * dto.Limit)
                .Take(dto.Limit)
                .ProjectTo<AuditCheckDetailResDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            var result = new IndexResDto<AuditCheckDetailResDto>()
            {
                Data = list,
                Limit = dto.Limit,
                Page = dto.Page,
                Total = total
            };
            return result;
        }
        [Display(Name = "چک کردن جداول")]
        [HttpGet("[action]")]
        public async Task<ApiResult<AuditCheckResDto>> AuditsCheck(CancellationToken ct)
        {
            var userId = User.Identity!.GetUserId<int>();
            var auditCheck = await _hashEntityValidator.ValidateAllHashedEntitiesAsync(userId, ct);
            var dto = AuditCheckResDto.FromEntity(auditCheck, _mapper);

            return Ok(dto);
        }

        [Display(Name = "دانلود نمونه فایل")]
        [HttpPost("[action]")]
        public IActionResult DownloadTemplateFile([FromQuery] string file)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates");
            var filePath = "";
            if (file == "issueTemplate")
            {
                filePath = Path.Combine(path, "issueTemplate.xlsx");
            }else if (file == "bankTransactionTemplate")
            {
                filePath = Path.Combine(path, "bankTransactionTemplate.xlsx");
            }
            if (!System.IO.File.Exists(filePath))
            {
                throw new NotFoundException("فایل پیدا نشد.");
            }
            var contentType = "application/octet-stream"; // فایل باینری
            return PhysicalFile(filePath, contentType, file + ".xlsx");
            
        }

        [Display(Name = "تنظیم لایسنس")]
        [HttpPost("[action]")]
        public async Task<ApiResult> SetLicense(int days, CancellationToken ct)
        {
            var userId = User.Identity!.GetUserId<int>();
            if (userId != 1)
                throw new AppException(ApiResultStatusCode.UnAuthorized, "کاربر دسترسی ندارد");
            var license = LicenseService.GenerateLicense(days);
            await _licenseService.SaveLicenseAsync(license, ct);

            return Ok();
        }
        
    }
}
