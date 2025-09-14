using Shared.Contracts;

namespace WebClient.Services.Common;

public static class FormDiscovery
{
    public static List<ICustomForm> DiscoverForms()
    {
        var forms = new List<ICustomForm>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in assemblies)
        {
            var types = asm.GetTypes()
                .Where(t => typeof(ICustomForm).IsAssignableFrom(t) &&
                            !t.IsInterface && !t.IsAbstract);

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is ICustomForm instance)
                {
                    forms.Add(instance);
                }
            }
        }

        return forms;
    }
}