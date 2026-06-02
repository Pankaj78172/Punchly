using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Punchly.Data;
using Punchly.Dtos;
using Punchly.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Punchly.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AccountController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register-personal")]
    public async Task<IActionResult> RegisterPersonalUser(RegisterPersonalUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("First name, last name, email, and password are required.");
        }

        var emailExists = await _context.AppUser
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            return BadRequest("This email is already registered.");
        }

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            IsActive = true
        };

        var passwordHasher = new PasswordHasher<AppUser>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var workspace = new Workspace
        {
            Name = $"{request.FirstName} {request.LastName}'s Workspace",
            Type = "Personal"
        };

        _context.AppUser.Add(user);
        _context.Workspaces.Add(workspace);

        await _context.SaveChangesAsync();

        var member = new WorkspaceMember
        {
            AppUserId = user.AppUserId,
            WorkSpaceID = workspace.WorkspaceId,
            Role = "Owner"
        };

        _context.WorkspacesMember.Add(member);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Personal account created successfully.",
            userId = user.AppUserId,
            workspaceId = workspace.WorkspaceId,
            workspaceType = workspace.Type
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.AppUser
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var passwordHasher = new PasswordHasher<AppUser>();

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.AppUserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
    };

        var secretKey = _configuration["JwtSettings:SecretKey"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            message = "Login successful.",
            token = tokenString,
            user = new
            {
                userId = user.AppUserId,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email
            }
        });
    }
}