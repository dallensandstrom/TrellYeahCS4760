namespace TrellYeahCapstone.Models
{
    public class AllocationGrantSummaryViewModel
    {
        public int GrantId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PrincipalInvestigatorName { get; set; } = string.Empty;

        public decimal MoneyRequestedFromArcc { get; set; }

        public decimal MoneyRequestedFromOtherSources { get; set; }

        public decimal? AverageScorePercentage { get; set; }

        public int ReviewerCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
