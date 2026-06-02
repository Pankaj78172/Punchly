using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Punchly.Api.Dtos;
using Punchly.Data;
using Punchly.Models;

namespace Punchly.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TimeEntriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TimeEntriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("punch-in")]
    public async Task<IActionResult> PunchIn(PunchInRequest request)
    {
        if (request.AppUserId <= 0 || request.WorkspaceId <= 0)
        {
            return BadRequest("User and workspace are required.");
        }

        if (!string.IsNullOrWhiteSpace(request.Note) && request.Note.Length > 250)
        {
            return BadRequest("Note cannot be more than 250 characters.");
        }

        var userExists = await _context.AppUser
            .AnyAsync(u => u.AppUserId == request.AppUserId && u.IsActive);

        if (!userExists)
        {
            return NotFound("User not found or inactive.");
        }

        var workspaceExists = await _context.Workspaces
            .AnyAsync(w => w.WorkspaceId == request.WorkspaceId);

        if (!workspaceExists)
        {
            return NotFound("Workspace not found.");
        }

        var isMember = await _context.WorkspacesMember
            .AnyAsync(m =>
                m.AppUserId == request.AppUserId &&
                m.WorkSpaceID == request.WorkspaceId);

        if (!isMember)
        {
            return BadRequest("User does not belong to this workspace.");
        }

        var openEntry = await _context.TimeEntries
            .FirstOrDefaultAsync(t =>
                t.AppUserId == request.AppUserId &&
                t.WorkspaceId == request.WorkspaceId &&
                t.PunchOutTime == null);

        if (openEntry != null)
        {
            return BadRequest("You are already punched in. Please punch out first.");
        }

        var entry = new TimeEntry
        {
            AppUserId = request.AppUserId,
            WorkspaceId = request.WorkspaceId,
            PunchInTime = DateTime.UtcNow,
            Note = request.Note
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Punch in successful.",
            timeEntryId = entry.TimeEntryId,
            punchInTime = entry.PunchInTime
        });
    }

    [HttpPost("punch-out")]
    public async Task<IActionResult> PunchOut(PunchOutRequest request)
    {
        if (request.AppUserId <= 0 || request.WorkspaceId <= 0)
        {
            return BadRequest("User and workspace are required.");
        }

        var openEntry = await _context.TimeEntries
            .FirstOrDefaultAsync(t =>
                t.AppUserId == request.AppUserId &&
                t.WorkspaceId == request.WorkspaceId &&
                t.PunchOutTime == null);

        if (openEntry == null)
        {
            return BadRequest("You are not punched in.");
        }

        var punchOutTime = DateTime.UtcNow;

        if (punchOutTime <= openEntry.PunchInTime)
        {
            return BadRequest("Punch out time must be after punch in time.");
        }

        openEntry.PunchOutTime = punchOutTime;

        var totalHours = (decimal)(punchOutTime - openEntry.PunchInTime).TotalHours;
        openEntry.TotalHours = Math.Round(totalHours, 2);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Punch out successful.",
            timeEntryId = openEntry.TimeEntryId,
            punchInTime = openEntry.PunchInTime,
            punchOutTime = openEntry.PunchOutTime,
            totalHours = openEntry.TotalHours
        });
    }

    [HttpGet("my-hours")]
    public async Task<IActionResult> GetMyHours(int appUserId, int workspaceId)
    {
        if (appUserId <= 0 || workspaceId <= 0)
        {
            return BadRequest("User and workspace are required.");
        }

        var entries = await _context.TimeEntries
            .Where(t => t.AppUserId == appUserId && t.WorkspaceId == workspaceId)
            .OrderByDescending(t => t.PunchInTime)
            .Select(t => new
            {
                t.TimeEntryId,
                t.PunchInTime,
                t.PunchOutTime,
                t.TotalHours,
                t.Note
            })
            .ToListAsync();

        var totalHours = entries
            .Where(e => e.TotalHours.HasValue)
            .Sum(e => e.TotalHours!.Value);

        return Ok(new
        {
            totalHours,
            entries
        });
    }


}