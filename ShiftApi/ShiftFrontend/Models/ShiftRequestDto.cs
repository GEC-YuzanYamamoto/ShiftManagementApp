namespace ShiftFrontend.Models
{
    public class ShiftRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateShiftRequestDto
    {
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
    }
}
