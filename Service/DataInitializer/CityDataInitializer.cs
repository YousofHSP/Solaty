using Data.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Service.DataInitializer
{
    public class CityDataInitializer(IRepository<City> Repository) : IDataInitializer
    {
        public async Task InitializerData()
        {
            var list = new List<City>()
            {
                new City() {
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
