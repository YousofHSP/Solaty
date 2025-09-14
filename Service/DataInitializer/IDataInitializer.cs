using Common;

namespace Service.DataInitializer;

public interface IDataInitializer: IScopedDependency
{
    public Task InitializerData();
}