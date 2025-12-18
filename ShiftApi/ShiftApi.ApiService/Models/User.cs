namespace ShiftApi.ApiService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public byte Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
        public ICollection<ShiftSchedule> ShiftSchedules { get; set; } = new List<ShiftSchedule>();
        public ICollection<ShiftSchedule> ConfirmedSchedules { get; set; } = new List<ShiftSchedule>();
    }
}
