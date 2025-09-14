using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum ApiResultStatusCode
    {
        [Display(Name = "عملیات با موفقیت انجام شد.")]
        Success = 200,

        [Display(Name = "با موفیت ایجاد شد.")]
        Created = 201,

        [Display(Name = "خطایی در سرور رخ داده است.")]
        ServerError = 500,

        [Display(Name = "پارامترهای ارسالی معتبر نیستند")]
        BadRequest = 400,

        [Display(Name = "یافت نشد")]
        NotFound = 404,

        [Display(Name = "لیست خالی است.")]
        ListEmpty = 204,

        [Display(Name = "خطایی در پردازش رخ داده است.")]
        LogicError = 409,

        [Display(Name = "شما اجازه دسترسی ندارید.")]
        UnAuthorized = 401,
        
        [Display(Name = "موجودی کافی نیست")]
        PaymentRequired = 402,

        [Display(Name = "کاربر مجاز نیست")]
        Forbidden= 403,

        [Display(Name = "تداخل دارد")]
        Conflict= 409,
    }
}
