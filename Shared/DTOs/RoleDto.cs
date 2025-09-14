using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Shared.DTOs
{
    public class RoleDto: BaseDto<RoleDto, Role>
    {

        [Display(Name = "عنوان")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Title { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Name { get; set; }

        [Display(Name = "دسترسی ها")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public List<string> Permissions { get; set; } = [];
    }
    public class RoleResDto : BaseDto<RoleResDto, Role>
    {
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Display(Name = "نام")]
        public string Name { get; set; }

        [Display(Name = "دسترسی ها")]
        public List<string> Permissions { get; set; }

    }
}
