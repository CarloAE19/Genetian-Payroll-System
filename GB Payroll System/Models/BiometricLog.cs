using System;

namespace GB_Payroll_System.Models
{
    public class BiometricLog
    {
        public int Id { get; set; }
        public string BiometricUserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string PunchType { get; set; } = "IN"; // IN, OUT, BREAK_IN, BREAK_OUT
        public string DeviceIp { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
