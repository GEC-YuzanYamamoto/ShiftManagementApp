namespace ShiftApi.ApiService.Models.DTOs
{
    public class ShiftScheduleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public int ConfirmedBy { get; set; }

    }

    public class CreateShiftScheduleDto
    {
        public int UserId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
    }

    public class UpdateShiftScheduleDto
    {
        public byte ShiftType { get; set; }
    }
}
