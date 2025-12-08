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
    [Route("shift-requests")]
    [Authorize]
    public class ShiftRequestController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ShiftRequestController(AppDbContext db)
        {
            _db = db;
        }

        // GET /shift-requests
        [HttpGet]
        public async Task<ActionResult<List<ShiftRequestDto>>> Get(
           [FromQuery] DateOnly? from,
           [FromQuery] DateOnly? to,
           [FromQuery] int? userId)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var query = _db.ShiftRequests.AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(r => r.ShiftDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(r => r.ShiftDate <= to.Value);
            }

            if (isAdmin)
            {
                if (userId.HasValue)
                {
                    query = query.Where(r => r.UserId == userId.Value);
                }
                // userId 指定なしなら全員分
            }
            else
            {
                // 一般ユーザーは自分の分だけ
                query = query.Where(r => r.UserId == currentUserId);
            }

            var list = await query
                .OrderBy(r => r.ShiftDate)
                .ThenBy(r => r.UserId)
                .ToListAsync();

            var result = list.Select(ToDto).ToList();
            return Ok(result);
        }

        // GET /shift-requests/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShiftRequestDto>> GetById(int id)
        {
            var entity = await _db.ShiftRequests.FindAsync(id);
            if (entity == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && entity.UserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(ToDto(entity));
        }

        // POST /shift-requests
        // ログイン中のユーザーが「希望を提出 / 再提出」する
        // 同じ日付の自分のレコードがあれば更新扱い
        [HttpPost]
        public async Task<ActionResult<ShiftRequestDto>> Create([FromBody] CreateShiftRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            var existing = await _db.ShiftRequests
                .FirstOrDefaultAsync(r => r.UserId == currentUserId && r.ShiftDate == dto.ShiftDate);

            if (existing != null)
            {
                // 再提出扱いで上書き
                existing.ShiftType = dto.ShiftType;
                existing.UpdatedAt = DateTime.UtcNow;

                // 再提出したので、ステータスを「未承認」に戻す
                existing.Status = 0; // Pending

                await _db.SaveChangesAsync();
                return Ok(ToDto(existing));
            }

            var entity = new ShiftRequest
            {
                UserId = currentUserId,
                ShiftDate = dto.ShiftDate,
                ShiftType = dto.ShiftType,

                // 新規提出は「未承認」
                Status = 0
            };

            _db.ShiftRequests.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
        }

        // PUT /shift-requests/{id}
        // 自分の希望変更 or 管理者による修正
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ShiftRequestDto>> Update(int id, [FromBody] UpdateShiftRequestDto dto)
        {
            var entity = await _db.ShiftRequests.FindAsync(id);
            if (entity == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && entity.UserId != currentUserId)
            {
                return Forbid();
            }

            entity.ShiftType = dto.ShiftType;
            entity.UpdatedAt = DateTime.UtcNow;

            // 変更したら再度「未承認」にする運用
            entity.Status = 0; // Pending

            await _db.SaveChangesAsync();

            return Ok(ToDto(entity));
        }

        // DELETE /shift-requests/{id}
        // 希望の取り消し
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.ShiftRequests.FindAsync(id);
            if (entity == null) return NotFound();

            // 自分の分 or 管理者のみ削除可能
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && entity.UserId != currentUserId)
            {
                return Forbid();
            }

            _db.ShiftRequests.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 管理者用：希望を「承認済み」にするエンドポイント
        // 例：フロントの Api.MarkRequestAsApprovedAsync(id) から呼ぶ想定
        // POST /shift-requests/{id}/approve
        [HttpPost("{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var entity = await _db.ShiftRequests.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Status = 1;            // Approved
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET /shift-requests/status?date=2025-05-10
        // 管理者向け：ある日の「提出済み / 未提出」一覧
        [HttpGet("status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ShiftSubmissionStatusDto>>> GetSubmissionStatus([FromQuery] DateOnly date)
        {
            // 全ユーザーを取得
            var users = await _db.Users
                .OrderBy(u => u.Id)
                .ToListAsync();

            // 指定日の希望一覧を取得
            var requests = await _db.ShiftRequests
                .Where(r => r.ShiftDate == date)
                .ToListAsync();

            // UserId → ShiftRequest の辞書
            var requestDict = requests.ToDictionary(r => r.UserId, r => r);

            var results = new List<ShiftSubmissionStatusDto>();

            foreach (var user in users)
            {
                if (requestDict.TryGetValue(user.Id, out var req))
                {
                    // 提出済み
                    results.Add(new ShiftSubmissionStatusDto
                    {
                        UserId = user.Id,
                        ShiftDate = date,
                        ShiftType = req.ShiftType.ToString(),
                        Status = "submitted"
                    });
                }
                else
                {
                    // 未提出
                    results.Add(new ShiftSubmissionStatusDto
                    {
                        UserId = user.Id,
                        ShiftDate = date,
                        ShiftType = null,
                        Status = "not_submitted"
                    });
                }
            }

            return Ok(results);
        }

        // ヘルパー
        // 現在のログインユーザーの UserId を取得
        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idClaim == null)
            {
                throw new InvalidOperationException("UserId claim (NameIdentifier) が見つかりません。");
            }

            return int.Parse(idClaim.Value);
        }

        // Status も DTO に含めて返す
        private static ShiftRequestDto ToDto(ShiftRequest r) =>
            new()
            {
                Id = r.Id,
                UserId = r.UserId,
                ShiftDate = r.ShiftDate,
                ShiftType = r.ShiftType,
                CreatedAt = r.CreatedAt,
                Status = r.Status
            };
    }
}
