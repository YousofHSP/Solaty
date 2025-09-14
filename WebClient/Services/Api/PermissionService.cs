namespace WebClient.Services.Api;

public interface IPermissionService
{
    bool HasPermission(string permission);
    List<string> GetAll();
    void SetPermissions(List<string> list);
}

public class PermissionService : IPermissionService
{
    private List<string> _permissions = new();

    public bool HasPermission(string permission)
    {

        Console.WriteLine("chekc permissions");
        foreach (var item in _permissions)
        {
            Console.WriteLine(item);
        }
        return _permissions.Contains(permission);
    }

    public List<string> GetAll() => _permissions;
    public void SetPermissions(List<string> list)
    {
        _permissions = list;
        
    }
}