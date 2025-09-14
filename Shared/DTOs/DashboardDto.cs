namespace Shared.DTOs;

public class DashboardDto 
{
}
public class DashboardCardsDto 
{
    public int TotalOngoingRequest { get; set; }
    public int TotalCompletedAfterMaxTimeRequest { get; set; }
    public int TotalCompletedInNormalTimeRequest { get; set; }
}
