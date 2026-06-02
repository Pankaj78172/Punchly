using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Globalization;

namespace Punchly.Models
{
    public class WorkspaceMember
    {
        public int WorkspaceMemberID { get; set; }
        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null;

        public int WorkSpaceID { get; set; }

        public Workspace Workspace { get; set; } = null!;

        public string Role { get; set; } = "Employee";
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    }
}
