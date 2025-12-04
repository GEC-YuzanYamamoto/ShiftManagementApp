using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftApi.ApiService.Data;
using ShiftApi.ApiService.Models;
using ShiftApi.ApiService.Models.DTOs;
using ShiftApi.ApiService.Services;

namespace ShiftApi.ApiService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet] // GET /users

        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
            .Select(u => new { u.Id, u.Name, u.Email, u.Role }) //匿名型に変換、パスワードなどの機密情報を除外
            .ToListAsync();

            return Ok(users);
        }

        [HttpPost] // POST /users
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
            {
                return BadRequest("既に使われているメールアドレスです");
            }

            var hash = PasswordHasher.Hash(dto.Password);


            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = hash,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created($"/users/{user.Id}", new { user.Id, user.Name, user.Email, user.Role });
        }
    }
}
