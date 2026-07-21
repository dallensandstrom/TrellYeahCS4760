namespace TrellYeahCapstone.Models
{
    public class ArccGrantSearchResultViewModel
    {
        public int GrantId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string CollegeName { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string PrincipalInvestigatorName { get; set; } = string.Empty;

        public string PrincipalInvestigatorEmail { get; set; } = string.Empty;

        public string ProjectDirectorName { get; set; } = string.Empty;

        public string ProjectDirectorEmail { get; set; } = string.Empty;

        public decimal MoneyRequestedFromArcc { get; set; }

        public decimal AllocatedAmount { get; set; }

        public decimal? AverageScorePercentage { get; set; }

        public int ReviewerCount { get; set; }

        public DateTime? SubmittedAt { get; set; }
    }
}
