using System;

namespace GB_Payroll_System.Models
{
    public enum HolidayType
    {
        RegularHoliday = 1,      // 200% worked, 100% unworked
        SpecialNonWorking = 2,   // 130% worked, 0% unworked (no work no pay)
        SpecialWorking = 3,      // 100% worked (regular rate)
        LocalSpecialHoliday = 4  // Management or LGU custom multiplier
    }

    public class Holiday
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "Manila Day", "Independence Day"
        public DateTime Date { get; set; }
        
        public HolidayType Type { get; set; } = HolidayType.RegularHoliday;
        
        public decimal WorkedMultiplier { get; set; } = 2.00m;    // e.g. 2.00 (200%), 1.30 (130%)
        public decimal UnworkedMultiplier { get; set; } = 1.00m;  // e.g. 1.00 (100%), 0.00 (0%)

        public int? BranchId { get; set; } // Null for nationwide, or specific branch for local holidays
        public string DeclaredBy { get; set; } = string.Empty; // e.g. Proclamation / LGU Mayor
        public bool IsActive { get; set; } = true;
    }
}
