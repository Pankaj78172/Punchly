using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Punchly.Api.Dtos;
using Punchly.Data;
using Punchly.Models;
using System.Security.Claims;

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

    private int GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            throw new UnauthorizedAccessException("User ID was not found in token.");
        }

        return int.Parse(userIdValue);
    }

    [HttpPost("punch-in")]
    public async Task<IActionResult> PunchIn(PunchInRequest request)
    {
        var appUserId = GetCurrentUserId();

        if (request.WorkspaceId <= 0)
        {
            return BadRequest("Workspace is required.");
        }

        if (!string.IsNullOrWhiteSpace(request.Note) && request.Note.Length > 250)
        {
            return BadRequest("Note cannot be more than 250 characters.");
        }

        var userExists = await _context.AppUser
            .AnyAsync(u => u.AppUserId == appUserId && u.IsActive);

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
                m.AppUserId == appUserId &&
                m.WorkSpaceID == request.WorkspaceId);

        if (!isMember)
        {
            return BadRequest("User does not belong to this workspace.");
        }

        var openEntry = await _context.TimeEntries
            .FirstOrDefaultAsync(t =>
                t.AppUserId == appUserId &&
                t.WorkspaceId == request.WorkspaceId &&
                t.PunchOutTime == null);

        if (openEntry != null)
        {
            return BadRequest("You are already punched in. Please punch out first.");
        }

        var entry = new TimeEntry
        {
            AppUserId = appUserId,
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
        var appUserId = GetCurrentUserId();

        if (request.WorkspaceId <= 0)
        {
            return BadRequest("Workspace is required.");
        }

        var openEntry = await _context.TimeEntries
            .FirstOrDefaultAsync(t =>
                t.AppUserId == appUserId &&
                t.WorkspaceId == request.WorkspaceId &&
                t.PunchOutTime == null);

        if (openEntry == null)
        {
            return BadRequest("You are not punched in.");
        }

        var punchOutTime = DateTime.UtcNow;

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
    public async Task<IActionResult> GetMyHours(int workspaceId)
    {
        var appUserId = GetCurrentUserId();

        if (workspaceId <= 0)
        {
            return BadRequest("Workspace is required.");
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