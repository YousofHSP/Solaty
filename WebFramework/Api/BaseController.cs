using System.Linq.Dynamic.Core;
using Domain.Entities.Common;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using WebFramework.Filters;

namespace WebFramework.Api
{

    [ApiController]
    [ApiResultFilter]
    [Route("api/v{version:ApiVersion}/[controller]")]
    public class BaseController<TKey> : ControllerBase
    {

        protected IQueryable<TEntity> setFilters<TEntity>(List<FilterDto>? filters, IQueryable<TEntity> query)
        {

            if (filters is null)
                return query;
            foreach (var filter in filters)
            {
                if (!string.IsNullOrWhiteSpace(filter.Field) &&
                    !string.IsNullOrWhiteSpace(filter.Operator) &&
                    !string.IsNullOrWhiteSpace(filter.Value))
                {
                    var allowedOperators = new[] { "==", "!=", ">", "<", ">=", "<=", "Contains" };
                    if (!allowedOperators.Contains(filter.Operator))
                        continue;
                    var property = typeof(TEntity).GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, filter.Field, StringComparison.OrdinalIgnoreCase));
                    if (property == null) continue;

                    object? typedValue;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(filter.Value) && Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            typedValue = null;
                        }
                        else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                        {
                            typedValue = bool.TryParse(filter.Value, out var b) ? b : null;
                        }
                        else
                        {
                            typedValue = Convert.ChangeType(filter.Value, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    var expression = filter.Operator == "Contains"
                        ? $"{filter.Field}.Contains(@0)"
                        : $"{filter.Field} {filter.Operator} @0";

                    query = query.Where(expression, typedValue);
                }
            }
            return query;
        }
        virtual protected IQueryable<TEntity> setSort<TEntity>(IndexSortDto sort, IQueryable<TEntity> query)
            where TEntity : IEntity<TKey>
        {
            if (sort.Type == "Desc" || sort.Type == "")
            {
                query = query.OrderBy($"{sort.By} descending");
            }
            else
            {
                query = query.OrderBy($"{sort.By}");
            }
            return query;
        }
    }
    [ApiController]
    [ApiResultFilter]
    [Route("api/v{version:ApiVersion}/[controller]")]
    public class BaseController : BaseController<int> { }
}