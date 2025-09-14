using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs;

public class AuditDto : BaseDto<AuditDto, Audit>
{

    [Display(Name = "Ip")]
    public string Ip { get; set; }
    [Display(Name = "لینک فعلی")]
    public string CurrentLink { get; set; }
    [Display(Name = "ارجاع‌دهنده")]
    public string ReferrerLink { get; set; }
    public string Protocol { get; set; }
    public string PhysicalPath { get; set; }
    [Display(Name = "مدل")]
    public string Model { get; set; }
    [Display(Name = "شناسه کاربر")]
    public int UserId { get; set; }
    [Display(Name = "متد")]
    public string Method { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    [Display(Name = "تاریخ")]
    public DateTimeOffset CreatedDate { get; set; }
    public string Browser { get; set; }
    public string OperationSystem { get; set; }
}