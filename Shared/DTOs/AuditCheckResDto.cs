using AutoMapper;
using Common.Utilities;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class AuditCheckResDto : BaseDto<AuditCheckResDto, AuditCheck>
    {
        [Display(Name = "تاریخ")]
        public string CreateDate { get; set; }
        [Display(Name = "تعداد جداول")]
        public int TablesCheckCount { get; set; }
        [Display(Name = "کاربر")]
        public string UserName { get; set; }
        [Display(Name = "تعداد موجودیت‌ها")]
        public int EntitiesCheckCount { get; set; }
        [Display(Name = "تعداد بازیابی‌شده‌ها")]
        public int RestoredEntitiesCount { get; set; }
        protected override void CustomMappings(IMappingExpression<AuditCheck, AuditCheckResDto> mapping)
        {
            mapping.ForMember(
                d => d.CreateDate,
                s => s.MapFrom(m => m.CreateDate.ToShamsi(true)));
            mapping.ForMember(
                d => d.UserName,
                s => s.MapFrom(m => m.CreatorUser.UserName));
            mapping.ForMember(
                d => d.EntitiesCheckCount,
                s => s.MapFrom(m => m.AuditCheckDetails.Count));
            mapping.ForMember(
                d => d.RestoredEntitiesCount,
                s => s.MapFrom(m => m.AuditCheckDetails.Count(i => i.Status == AuditCheckDetailStatus.Invalid)));


        }
    }

    public class AuditCheckDetailResDto : BaseDto<AuditCheckDetailResDto, AuditCheckDetail>
    {
        [Display(Name = "مدل")]
        public string Model { get; set; }
        [Display(Name = "شناسه مدل")]
        public int ModelId { get; set; }
        [Display(Name = "وضعیت")]
        public AuditCheckDetailStatus Status{ get; set; }
        [Display(Name = "تاریخ")]
        public string AuditCreateDate { get; set; }

        protected override void CustomMappings(IMappingExpression<AuditCheckDetail, AuditCheckDetailResDto> mapping)
        {
            mapping.ForMember(
                d => d.AuditCreateDate,
                s => s.MapFrom(m => m.AuditCreateDate != null ? m.AuditCreateDate.Value.ToShamsi(true) : "-"));
        }

    }

}
