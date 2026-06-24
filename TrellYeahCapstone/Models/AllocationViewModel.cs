using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class AllocationViewModel
    {
        public List<GrantAllocation> PastAllocations { get; set; } = [];

        public List<AllocationGrantSummaryViewModel> SubmittedGrants { get; set; } = [];

        public List<AllocationGrantSummaryViewModel> RejectedGrants { get; set; } = [];

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be 0 or greater.")]
        [Display(Name = "Money available")]
        public decimal CurrentRoundAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be 0 or greater.")]
        [Display(Name = "Money from last round")]
        public decimal PreviousRoundAmount { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Cutoff percentage must be between 0 and 100.")]
        [Display(Name = "Cutoff percentage (0-100%)")]
        public decimal CutoutPercentage { get; set; }
    }
}
