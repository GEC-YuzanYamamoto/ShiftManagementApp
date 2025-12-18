namespace ShiftApi.ApiService.Models
{
    public class ShiftSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;
        public int ConfirmedBy { get; set; }
        public User ConfirmedByUser { get; set; } = null!;
    }
}
