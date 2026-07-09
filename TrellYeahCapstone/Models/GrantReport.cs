using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrellYeahCapstone.Models
{
    public class GrantReport
    {
        public int GrantReportId { get; set; }

        public int GrantId { get; set; }

        public TrellYeahCS4760.Models.Grant? Grant { get; set; }

        public DateTime SubmissionDate { get; set; }

        [Required]
        [StringLength(5000)]
        public string ProjectSummary { get; set; } = string.Empty;


        [Required]
        [StringLength(5000)]
        public string CurrentProgress { get; set; } = string.Empty;


        [Required]
        [StringLength(5000)]
        public string NextSteps { get; set; } = string.Empty;


        [Required]
        [StringLength(5000)]
        public string Budget { get; set; } = string.Empty;


        [StringLength(500)]
        public string? ReportFilePath { get; set; }

        [NotMapped]
        public IFormFile? ReportFile { get; set; }

    }
}
