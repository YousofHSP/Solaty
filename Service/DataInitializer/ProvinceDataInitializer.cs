using Data.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Service.DataInitializer
{
    public class ProvinceDataInitializer(IRepository<Province> Repository) : IDataInitializer
    {
        public async Task InitializerData()
        {
            var list = new List<Province>()
            {
                new Province() {
                    Title = "یزد",
                    Cities = new List<City>
                    {
                        new City{ Title = "یزد"}
                    }
                }
            };
            var any = await Repository.TableNoTracking.AnyAsync();
            if(!any)
                await Repository.AddRangeAsync(list, default);
        }
    }
}
