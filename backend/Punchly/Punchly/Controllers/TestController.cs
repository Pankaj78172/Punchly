using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Punchly.Data;

namespace Punchly.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var canConnect = await _context.Database.CanConnectAsync();

        return Ok(new
        {
            message = "Punchly API is working",
            databaseConnected = canConnect,
            appName = "Punchly"
        });
    }
}