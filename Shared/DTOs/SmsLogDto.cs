using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Common.Utilities;
using Domain.Entities;

namespace Shared.DTOs
{
    public class SmsLogResDto : BaseDto<SmsLogResDto, SmsLog>
    {
        [Display(Name = "متن پیام")]
        public string Text { get; set; }
        [Display(Name = "تاریخ")]
        public string CreateDate { get; set; }
        [Display(Name = "کاربر")]
        public string CreatorUserName { get; set; }
        [Display(Name = "شماره موبایل")]
        public string Mobile { get; set; }

        protected override void CustomMappings(IMappingExpression<SmsLog, SmsLogResDto> mapping)
        {
            mapping.ForMember(
                d => d.CreatorUserName,
                s => s.MapFrom(m => m.CreatorUser.UserName));
            mapping.ForMember(
                d => d.CreateDate,
                s => s.MapFrom(m => m.CreateDate.ToShamsi(default)));
        }

    }
    public class ReSendDto
    {
        [Required]
        public long Id { get; set; }
    }


}
