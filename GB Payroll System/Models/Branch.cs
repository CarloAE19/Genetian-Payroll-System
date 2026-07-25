namespace GB_Payroll_System.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // e.g. Quezon City, Cebu City, Davao
        public bool IsActive { get; set; } = true;
    }
}
