using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftApi.ApiService.Data;
using ShiftApi.ApiService.Models;
using ShiftApi.ApiService.Services;

namespace ShiftApi.ApiService.Controllers;

public record CreateAdminRequest(string Name, string Email, string Password);

[ApiController]
[Route("setup")]
public class SetupController : ControllerBase
{
    private readonly AppDbContext _db;
    public SetupController(AppDbContext db) => _db = db;


    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult> Status()
    {
        const byte AdminRole = 1;

        var adminExists = await _db.Users.AnyAsync(u => u.Role == AdminRole);
        return Ok(new { needsSetup = !adminExists });
    }

    [HttpPost("admin")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateAdmin([FromBody] CreateAdminRequest req)
    {
        const byte AdminRole = 1;

        // すでに管理者がいるなら、初期セットアップは終了しているので拒否
        var adminExists = await _db.Users.AnyAsync(u => u.Role == AdminRole);
        if (adminExists) return Conflict(new { message = "Setup already completed." });

        var hash = PasswordHasher.Hash(req.Password);

        var admin = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = PasswordHasher.Hash(req.Password),
            Role = AdminRole
        };

        _db.Users.Add(admin);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Admin created." });
    }
}
