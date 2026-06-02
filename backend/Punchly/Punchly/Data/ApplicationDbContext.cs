using Microsoft.EntityFrameworkCore;
using Punchly.Models;

namespace Punchly.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { 
        
       }
        public DbSet<AppUser> AppUser {get; set; }
        public DbSet<Workspace> Workspaces {get; set; }
        public DbSet<WorkspaceMember> WorkspacesMember { get; set; }
        public DbSet<TimeEntry> TimeEntries {get; set; }

    }
}
