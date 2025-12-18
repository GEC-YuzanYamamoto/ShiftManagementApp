using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftApi.ApiService.Data;
using ShiftApi.ApiService.Models;
using ShiftApi.ApiService.Models.DTOs;
using System.Security.Claims;

namespace ShiftApi.ApiService.Controllers
{
    [ApiController]
    [Route("shift-schedules")]
    [Authorize]
    public class ShiftScheduleController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ShiftScheduleController(AppDbContext db)
        {
            _db = db;
        }

        // GET /shift-schedules?from=2025-01-01&to=2025-01-31&userId=1
        // 一般ユーザー：自分の確定シフトのみ
        // 管理者：userId 指定でその人、未指定なら全員分
        [HttpGet]
        public async Task<ActionResult<List<ShiftScheduleDto>>> Get(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int? userId)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var query = _db.ShiftSchedules.AsNoTracking().AsQueryable();

            if (from.HasValue) query = query.Where(s => s.ShiftDate >= from.Value);
            if (to.HasValue) query = query.Where(s => s.ShiftDate <= to.Value);

            if (isAdmin)
            {
                if (userId.HasValue)
                    query = query.Where(s => s.UserId == userId.Value);
            }
            else
            {
                query = query.Where(s => s.UserId == currentUserId);
            }

            var result = await query
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.UserId)
                .Select(s => new ShiftScheduleDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserName = s.User.Name,
                    ShiftDate = s.ShiftDate,
                    ShiftType = s.ShiftType,
                    ConfirmedAt = s.ConfirmedAt,
                    ConfirmedBy = s.ConfirmedBy,
                    ConfirmedByName = s.ConfirmedByUser.Name
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET /shift-schedules/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShiftScheduleDto>> GetById(int id)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            // まず Entity を取って権限チェック（軽量）
            var entity = await _db.ShiftSchedules.AsNoTracking()
                .Select(s => new { s.Id, s.UserId })
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null) return NotFound();
            if (!isAdmin && entity.UserId != currentUserId) return Forbid();

            // DTOとして取得（名前込み）
            var dto = await LoadDtoAsync(id);
            return Ok(dto);
        }

        // POST /shift-schedules
        // 管理者がシフトを確定（新規作成 or 上書き）
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShiftScheduleDto>> Create([FromBody] CreateShiftScheduleDto dto)
        {
            var adminId = GetCurrentUserId();

            var existing = await _db.ShiftSchedules
                .FirstOrDefaultAsync(s => s.UserId == dto.UserId && s.ShiftDate == dto.ShiftDate);

            if (existing != null)
            {
                existing.ShiftType = dto.ShiftType;
                existing.ConfirmedAt = DateTime.UtcNow;
                existing.ConfirmedBy = adminId;

                await DeleteRelatedRequestsAsync(dto.UserId, dto.ShiftDate);

                await _db.SaveChangesAsync();

                return Ok(await LoadDtoAsync(existing.Id));
            }

            var entity = new ShiftSchedule
            {
                UserId = dto.UserId,
                ShiftDate = dto.ShiftDate,
                ShiftType = dto.ShiftType,
                ConfirmedAt = DateTime.UtcNow,
                ConfirmedBy = adminId
            };

            _db.ShiftSchedules.Add(entity);

            await DeleteRelatedRequestsAsync(dto.UserId, dto.ShiftDate);

            await _db.SaveChangesAsync();

            // ★ここがバグ修正：existing ではなく entity の Id を使う
            var resultDto = await LoadDtoAsync(entity.Id);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, resultDto);
        }

        // PUT /shift-schedules/{id}
        // 管理者が確定シフトを変更
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShiftScheduleDto>> Update(
            int id,
            [FromBody] UpdateShiftScheduleDto dto)
        {
            var adminId = GetCurrentUserId();

            var entity = await _db.ShiftSchedules.FindAsync(id);
            if (entity == null) return NotFound();

            entity.ShiftType = dto.ShiftType;
            entity.ConfirmedAt = DateTime.UtcNow;
            entity.ConfirmedBy = adminId;

            await _db.SaveChangesAsync();

            return Ok(await LoadDtoAsync(entity.Id));
        }

        // DELETE /shift-schedules/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.ShiftSchedules.FindAsync(id);
            if (entity == null) return NotFound();

            _db.ShiftSchedules.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task DeleteRelatedRequestsAsync(int userId, DateOnly shiftDate)
        {
            var requests = await _db.ShiftRequests
                .Where(r => r.UserId == userId && r.ShiftDate == shiftDate)
                .ToListAsync();

            if (requests.Count > 0)
            {
                _db.ShiftRequests.RemoveRange(requests);
            }
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idClaim == null)
                throw new InvalidOperationException("UserId claim (NameIdentifier) が見つかりません。");

            return int.Parse(idClaim.Value);
        }

        private Task<ShiftScheduleDto> LoadDtoAsync(int id)
        {
            return _db.ShiftSchedules.AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new ShiftScheduleDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserName = s.User.Name,
                    ShiftDate = s.ShiftDate,
                    ShiftType = s.ShiftType,
                    ConfirmedAt = s.ConfirmedAt,
                    ConfirmedBy = s.ConfirmedBy,
                    ConfirmedByName = s.ConfirmedByUser.Name
                })
                .FirstAsync();
        }
    }
}
