namespace ShiftApi.ApiService.Models
{
    public class ShiftRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public byte Status { get; set; } = 0;
        // 0 = 未承認, 1 = 承認済み, 2 = 却下（必要なら）
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
