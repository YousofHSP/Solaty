using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.DTOs.CustomMapping;

public static class AutoMapperConfiguration
{
    public static void InitializeAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddAutoMapper(config => { config.AddCustomMappingProfile(); }, assemblies);
    }

    private static void AddCustomMappingProfile(this IMapperConfigurationExpression config)
    {
        config.AddCustomMappingProfile(typeof(IHaveCustomMapping).Assembly!);
    }

    private static void AddCustomMappingProfile(this IMapperConfigurationExpression config, params Assembly[] assemblies)
    {
        var allTypes = assemblies.SelectMany(a => a.ExportedTypes);

        var list = allTypes
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                           type.GetInterfaces().Contains(typeof(IHaveCustomMapping)))
            .Select(type => (IHaveCustomMapping)Activator.CreateInstance(type)!);
        config.AddProfile(new CustomMappingProfile(list));
    }
}