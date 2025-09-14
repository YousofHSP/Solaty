using AutoMapper;
using Common.Utilities;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class BackupDto : BaseDto<BackupDto, Backup>
    {
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }
        [Display(Name = "تاریخ")]
        public string Date { get; set; }
        [Display(Name = "نام فایل")]
        public string FileName { get; set; }

        protected override void CustomMappings(IMappingExpression<Backup, BackupDto> mapping)
        {
            mapping.ForMember(
                d => d.UserName,
                s => s.MapFrom(m => m.CreatorUser.UserName));

            mapping.ForMember(
                d => d.Date,
                s => s.MapFrom(m => m.CreateDate.ToShamsi(default)));
        }
    }
}
