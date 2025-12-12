namespace ShiftFrontend.Models
{
    public class ShiftScheduleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
    }

    public class ShiftManagementDto
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
