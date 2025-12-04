namespace ShiftApi.ApiService.Models
{
    public class ShiftRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
