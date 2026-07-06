using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class GrantReportCreateViewModel
    {
        public int GrantId { get; set; }

        public string ProjectDirector { get; set; } = string.Empty;

        public string ProjectTitle { get; set; } = string.Empty;

        public int AccountNumber { get; set; }

        public DateTime SubmissionDate { get; set; }

        public DateTime? AwardDate { get; set; }

        [Required]
        public string ProjectSummary { get; set; } = string.Empty;

        [Required]
        public string CurrentProgress { get; set; } = string.Empty;

        [Required]
        public string NextSteps { get; set; } = string.Empty;

        [Required]
        public string Budget { get; set; } = string.Empty;

        public IFormFile? ReportFile { get; set; }
    }
}