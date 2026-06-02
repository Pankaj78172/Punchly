namespace Punchly.Models
{
    public class AppUser
    {
        public int AppUserId { get; set; }
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public List<WorkspaceMember> WorkspaceMembers { get; set; } = new();
        public List<TimeEntry> TimeEntries { get; set; } = new();
    }
}
