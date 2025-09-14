using AutoMapper;

namespace Domain.DTOs.CustomMapping;

public interface IHaveCustomMapping
{
    void CreateMappings(Profile profile);
    
}