using AutoMapper;
using Domain.DTOs.CustomMapping;
using Domain.Entities.Common;
using Shared.Attribute;

// namespace Common
// {
//     public class SelectDto
//     {
//         public object Id { get; set; }
//         public string Title { get; set; }
//         public bool Disabled { get; set; } = false;
//     }
// }

namespace Shared.DTOs
{
    public abstract class BaseDto<TDto, TEntity, TKey> : IHaveCustomMapping
        where TDto : class
        where TEntity : class, IEntity<TKey>
    {
        [ExcelIgnore]
        public  TKey? Id { get; set; }

        public TEntity ToEntity(IMapper mapper)
        {
            return mapper.Map<TEntity>(CastToDerivedClass(this, mapper));
        }
        public TEntity ToEntity(TEntity entity, IMapper mapper)
        {
            return mapper.Map(CastToDerivedClass(this, mapper), entity);
        }

        public static TDto FromEntity(TEntity model, IMapper mapper)
        {
            return mapper.Map<TDto>(model);
        }

        private static TDto CastToDerivedClass(BaseDto<TDto, TEntity, TKey> baseInstance, IMapperBase mapper)
        {
            return mapper.Map<TDto>(baseInstance);
        }
        public void CreateMappings(Profile profile)
        {
            var mappingExpression = profile.CreateMap<TDto, TEntity>();

            var dtoType = typeof(TDto);
            var entityType = typeof(TEntity);
            //Ignore any property of source (like Post.Author) that dose not contains in destination 
            foreach (var property in entityType.GetProperties())
            {
                if (dtoType.GetProperty(property.Name) == null)
                    mappingExpression.ForMember(property.Name, opt => opt.Ignore());
            }
            CustomerReverseMapping(mappingExpression);
            CustomMappings(mappingExpression.ReverseMap());
        }

        protected virtual void CustomerReverseMapping(IMappingExpression<TDto, TEntity> mapping)
        {

        }
        protected virtual void CustomMappings(IMappingExpression<TEntity, TDto> mapping)
        {
        }

    }
    public abstract class BaseDto<TDto, TEntity> : BaseDto<TDto, TEntity, long>
        where TDto : class
        where TEntity : class, IEntity<long>
    {
    }
}