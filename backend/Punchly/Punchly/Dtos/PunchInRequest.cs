namespace Punchly.Api.Dtos;

public class PunchInRequest
{
    public int AppUserId { get; set; }

    public int WorkspaceId { get; set; }

    public string? Note { get; set; }
}