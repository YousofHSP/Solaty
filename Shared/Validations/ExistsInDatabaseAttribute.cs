using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Data.Contracts;
using Domain.Entities.Common;

namespace Shared.Validations
{
    public class ExistsInDatabaseAttribute<TEntity> : ValidationAttribute
        where TEntity : class, IEntity
    {
        private readonly string _propertyName;

        public ExistsInDatabaseAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            // گرفتن Repository از DI
            var repository = (IRepository<TEntity>)validationContext.GetService(typeof(IRepository<TEntity>));
            if (repository == null)
                throw new InvalidOperationException($"Repository for {typeof(TEntity).Name} not found in service provider");

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, _propertyName);
            var constant = Expression.Constant(value);
            var equal = Expression.Equal(property, Expression.Convert(constant, property.Type));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);

            var exists = repository.Table.Any(lambda);
            var name = typeof(TEntity).GetCustomAttribute<DisplayAttribute>()?.Name ?? typeof(TEntity).Name;
            var errorMessage = $"{name} پیدا نشد";

            return exists
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? errorMessage);
        }
    }
}
