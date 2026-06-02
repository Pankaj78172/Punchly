namespace Punchly.Models
{
    public class Workspace
    {
        public int WorkspaceId{ get; set; }
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = "Personal";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<WorkspaceMember> WorkspaceMember { get; set; } = new();
        public List<TimeEntry> TimeEntry { get; set; } = new();

    }
}
