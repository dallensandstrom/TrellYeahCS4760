namespace TrellYeahCapstone.Models
{
    public class GrantReviewSummaryViewModel
    {
        public int GrantId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PrincipalInvestigatorName { get; set; } = string.Empty;

        public decimal MoneyRequestedFromArcc { get; set; }

        public DateTime? SubmittedAt { get; set; }
    }
}
