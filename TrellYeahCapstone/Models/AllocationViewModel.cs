using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class AllocationViewModel
    {
        public List<GrantAllocation> PastAllocations { get; set; } = [];

        public List<AllocationGrantSummaryViewModel> SubmittedGrants { get; set; } = [];

        public List<AllocationGrantSummaryViewModel> RejectedGrants { get; set; } = [];

        public List<AllocationGrantSummaryViewModel> AccountingGrants { get; set; } = [];

        public List<AllocationCriterion> AllocationCriteria { get; set; } = [];

        public bool CanFinishAllocating =>
            AllocationCriteria.Any() &&
            SubmittedGrants.Any(grant => grant.AllocatedAmount.HasValue);

        public bool CanSendToAccounting => AccountingGrants.Any();

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

        // Fields for adding a new allocation criterion
        [Range(0, 100, ErrorMessage = "Min score must be between 0 and 100.")]
        [Display(Name = "Min score %")]
        public decimal NewCriterionMinScore { get; set; }

        [Range(0, 100, ErrorMessage = "Max score must be between 0 and 100.")]
        [Display(Name = "Max score %")]
        public decimal NewCriterionMaxScore { get; set; }

        [Range(0, 100, ErrorMessage = "Allocation % must be between 0 and 100.")]
        [Display(Name = "Allocation %")]
        public decimal NewCriterionAllocationPercentage { get; set; }
    }
}
