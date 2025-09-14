using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Api.Controllers.v1;
using Data.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.DataInitializer;

namespace Api.DataInitializer
{
    public class LogCategoryDataInitializer : IDataInitializer
    {
        private readonly IRepository<LogCategory> _repository;

        public LogCategoryDataInitializer(IRepository<LogCategory> repository)
        {
            _repository = repository;
        }

        public async Task InitializerData()
        {
            var apiAssembly = typeof(UserController).Assembly;
            var controllerTypes = apiAssembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            var logCategories = new List<LogCategory>();
            foreach (var controllerType in controllerTypes)
            {
                var controllerName = controllerType.Name.Replace("Controller", "");

                var controllerDisplayAttr = controllerType.GetCustomAttribute<DisplayAttribute>();
                var controllerDisplayName = controllerDisplayAttr?.Name ?? controllerName;

                var methods = controllerType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => !m.IsSpecialName && !m.GetCustomAttributes<NonActionAttribute>().Any());

                foreach (var method in methods)
                {
                    if (method.DeclaringType == typeof(object))
                        continue;
                    //var declaringType = method.DeclaringType!;
                    //if (!typeof(CrudController<,,,>).IsAssignableFrom(declaringType) && declaringType != controllerType)
                    //    continue;
                    var actionName = method.Name;

                    var actionDisplayAttr = method.GetCustomAttribute<DisplayAttribute>();
                    var actionDisplayName = actionDisplayAttr?.Name ?? actionName;

                    // ذخیره در دیتابیس
                    logCategories.Add(new LogCategory
                    {
                        ControllerName = controllerName,
                        ControllerFaName = controllerDisplayName,
                        ActionName = actionName,
                        ActionFaName = actionDisplayName,
                    });
                }
            }
            var list = await _repository.TableNoTracking.ToListAsync();
            var models = logCategories.Where(i => list.All(l => i.ControllerName != l.ControllerName && i.ActionName != l.ActionName)).ToList();
            await _repository.AddRangeAsync(models, default);
        }
    }
}
