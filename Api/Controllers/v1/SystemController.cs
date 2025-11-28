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
        private readonly ILicenseService _licenseService;
        private readonly IMapper _mapper;

        public SystemController(IConfiguration configuration, IMapper mapper,
            ILicenseService licenseService)
        {
            _connectionString = configuration.GetConnectionString("SqlServer") ?? "";
            _mapper = mapper;
            _licenseService = licenseService;
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
            }
            else if (file == "bankTransactionTemplate")
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