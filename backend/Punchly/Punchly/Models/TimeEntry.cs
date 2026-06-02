namespace Punchly.Models
{
    public class TimeEntry
    {
        public  int TimeEntryId {get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        public int WorkspaceId { get; set; }

        public Workspace Workspace { get; set; } = null;
        public DateTime PunchInTime { get; set; }

        public DateTime? PunchOutTime { get; set; }
        public decimal? TotalHours { get; set; }
        public string? Note { get; set; }
        public bool IsEdited { get; set; } = false;
        public string? EditReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;




    }
}
