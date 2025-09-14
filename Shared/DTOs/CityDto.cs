using Domain.Entities;

namespace Shared.DTOs
{
    public  class CityDto : BaseDto<CityDto, City>
    {
    }
    public class CityResDto : BaseDto<CityResDto, City>
    {
        public string Title { get; set; }
        public string ProvinceTitle { get; set; }

    }
}
