namespace TrellYeahCapstone.Models
{
    public class GrantReviewSummaryViewModel
    {
        public int GrantId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PrincipalInvestigatorName { get; set; } = string.Empty;

        public decimal MoneyRequestedFromArcc { get; set; }
        public decimal MoneyRequestedFromCollege { get; set; }
        public decimal MoneyRequestedFromDepartment { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool HasSavedReview { get; set; }
    }
}
