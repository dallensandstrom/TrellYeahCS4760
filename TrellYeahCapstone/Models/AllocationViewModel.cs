using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class AllocationViewModel
    {
        public List<GrantAllocation> PastAllocations { get; set; } = [];
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be 0 or greater.")]
        [Display(Name = "Money available for this grant round")]
        public decimal CurrentRoundAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be 0 or greater.")]
        [Display(Name = "Money available from previous grant round")]
        public decimal PreviousRoundAmount { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Cutout percentage must be between 0 and 100.")]
        [Display(Name = "Cutout percentage (0–100%)")]
        public decimal CutoutPercentage { get; set; }
    }
}
