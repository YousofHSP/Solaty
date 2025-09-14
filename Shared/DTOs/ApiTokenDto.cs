using AutoMapper;
using Common.Utilities;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class ApiTokenResDto : BaseDto<ApiTokenResDto, ApiToken>
    {
        [Display(Name = "آی‌پی")]
        public string Ip { get; set; }
        [Display(Name = "مرورگر")]
        public string UserAgent { get; set; }
        [Display(Name = "تاریخ ایجاد")]
        public string CreateDate { get; set; }
        public string Code { get; set; }
        [Display(Name = "وضعیت")]
        public bool Enable{ get; set; }
        [Display(Name = "کاربر")]
        public string UserName { get; set; }
        [Display(Name = "آخرین استفاده")]
        public string LastUsedDate { get; set; }

        protected override void CustomMappings(IMappingExpression<ApiToken, ApiTokenResDto> mapping)
        {
            mapping.ForMember(
                d => d.CreateDate,
                s => s.MapFrom(m => m.CreateDate.ToShamsi(true)));
            mapping.ForMember(
                d => d.LastUsedDate,
                s => s.MapFrom(m => m.LastUsedDate.ToShamsi(true)));
            
            mapping.ForMember(
                d => d.UserName,
                s => s.MapFrom(m => m.User.UserName));
        }
    }
}
