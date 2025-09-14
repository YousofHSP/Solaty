namespace Shared.DTOs;
public class ProcessStep
{
    public string Id { get; set; }
    public string Title { get; set; }
}

public class ProcessConnection
{
    public string From { get; set; }
    public string To { get; set; }
}

public class ProcessDiagramDto
{
    public List<ProcessStep> Steps { get; set; } = new();
    public List<ProcessConnection> Connections { get; set; } = new();
}
