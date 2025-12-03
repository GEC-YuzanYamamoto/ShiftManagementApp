namespace ShiftApi.ApiService.Models
{
    public class ShiftRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
