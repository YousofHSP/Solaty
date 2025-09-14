using AutoMapper;
using Common.Utilities;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{

    public class ArchiveLogResDto : BaseDto<ArchiveLogResDto, ArchiveLog>
    {
        [Display(Name = "نام فایل")]
        public string ArchiveFileName { get; set; }
        [Display(Name = "آرشیو شده تا")]
        public string ArchivedUntilDate { get; set; }
        [Display(Name = "تاریخ آرشیو")]
        public string ArchivedAt { get; set; }
        [Display(Name = "تعداد لاگ")]
        public int LogCount { get; set; }
        [Display(Name = "تعداد ممیزی")]
        public int AuditCount { get; set; }

        protected override void CustomMappings(IMappingExpression<ArchiveLog, ArchiveLogResDto> mapping)
        {
            mapping.ForMember(
                d => d.ArchivedUntilDate,
                s => s.MapFrom(m => m.ArchivedUntilDate.ToShamsi(default)));
            mapping.ForMember(
                d => d.ArchivedAt,
                s => s.MapFrom(m => m.ArchivedAt.ToShamsi(default)));
        }
    }
    public class ArchiveRequestDto
    {
        public string UntilDate { get; set; }
    }
}
