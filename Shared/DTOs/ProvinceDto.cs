using Domain.Entities;

namespace Shared.DTOs
{
    public class ProvinceDto : BaseDto<ProvinceDto, Province>
    {
    }
    public class ProvinceResDto : BaseDto<ProvinceResDto, Province>
    {
        public string Title { get; set; }

    }
}
