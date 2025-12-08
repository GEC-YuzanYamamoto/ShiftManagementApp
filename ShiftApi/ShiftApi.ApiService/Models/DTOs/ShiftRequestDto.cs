namespace ShiftApi.ApiService.Models.DTOs
{
    public class ShiftRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class  CreateShiftRequestDto
    {
        public DateOnly ShiftDate { get; set; }
        public byte ShiftType { get; set; }
    }

    public class  UpdateShiftRequestDto
    {
        public byte ShiftType { get; set; }
    }

    public class  ShiftSubmissionStatusDto
    {
        public int UserId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public string? ShiftType { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
