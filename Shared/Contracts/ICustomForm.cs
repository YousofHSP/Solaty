namespace Shared.Contracts;

public interface ICustomForm
{
    public string Name { get; }
    Type ComponentType { get; }
}

public class FormResult
{
    public string Value { get; set; }
}