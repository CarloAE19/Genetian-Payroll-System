namespace GB_Payroll_System.Models
{
    public class PakyawRate
    {
        public int Id { get; set; }
        public string TaskCode { get; set; } = string.Empty; // e.g. TASK-PACK-01
        public string TaskName { get; set; } = string.Empty; // e.g. Box Packing
        public string UnitOfMeasure { get; set; } = string.Empty; // e.g. Box, Piece, Meter, Batch
        public decimal RatePerUnit { get; set; } // e.g. 25.00
        public bool IsActive { get; set; } = true;
    }
}
