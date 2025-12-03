namespace ShiftApi.ApiService.Models
{
    public class ShiftSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public int ConfirmedBy { get; set; }
    }
}
