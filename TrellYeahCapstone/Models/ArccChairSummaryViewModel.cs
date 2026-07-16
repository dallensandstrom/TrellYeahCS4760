namespace TrellYeahCapstone.Models
{
    public class ArccChairSummaryViewModel
    {
        public int TotalApplications { get; set; }
        public int AwardedGrantCount { get; set; }

        public decimal TotalAwarded { get; set; }
        public decimal TotalMatchingFunds { get; set; }
        public decimal AverageAward { get; set; }

        public int StudentsBenefited { get; set; }
        public int ReportsSubmitted { get; set; }
        public int ReportsOutstanding { get; set; }

        public List<ArccSummaryAmountRow> AwardsByCollege { get; set; } = new();
        public List<ArccSummaryAmountRow> AwardsByDepartment { get; set; } = new();
        public List<ArccSummaryAmountRow> FundingSources { get; set; } = new();

        public List<ArccSummaryCountRow> ApplicationStatuses { get; set; } = new();
        public List<ArccSummaryCountRow> ReportsByStatus { get; set; } = new();

        public List<ArccSummaryGrantRow> RecentAwards { get; set; } = new();
    }

    public class ArccSummaryAmountRow
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int GrantCount { get; set; }
    }

    public class ArccSummaryCountRow
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ArccSummaryGrantRow
    {
        public string Title { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public decimal AwardedAmount { get; set; }
        public int StudentsBenefited { get; set; }

        public DateTime? AwardDate { get; set; }
        public bool ReportSubmitted { get; set; }
    }
}