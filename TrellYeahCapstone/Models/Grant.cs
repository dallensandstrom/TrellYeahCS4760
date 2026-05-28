using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrellYeahCS4760.Models
{
    public class Grant
    {
        public int GrantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public string? UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Justification { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Project Director")]
        public string ProjectDirectorUserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Principal Investigator")]
        public string PrincipalInvestigatorUserId { get; set; } = string.Empty;

        [Display(Name = "How many Weber State students will benefit from this grant?")]
        [Range(0, int.MaxValue)]
        public int WeberStateStudentsBenefited { get; set; }

        [Display(Name = "Will this grant benefit more than one department?")]
        public bool BenefitsMultipleDepartments { get; set; }

        [Display(Name = "How many departments will benefit?")]
        public int? NumberOfDepartmentsBenefited { get; set; }

        [Display(Name = "Does this project use human subjects?")]
        public bool UsesHumanSubjects { get; set; }

        [StringLength(500)]
        public string? SupportingDocument1Path { get; set; }

        [StringLength(500)]
        public string? SupportingDocument2Path { get; set; }

        [StringLength(500)]
        public string? SupportingDocument3Path { get; set; }

        [StringLength(500)]
        public string? IRBApprovalFilePath { get; set; }

        [NotMapped]
        public List<SelectListItem> UserOptions { get; set; } = new();

        [NotMapped]
        public IFormFile? SupportingDocument1 { get; set; }

        [NotMapped]
        public IFormFile? SupportingDocument2 { get; set; }

        [NotMapped]
        public IFormFile? SupportingDocument3 { get; set; }

        [NotMapped]
        public IFormFile? IRBApprovalFile { get; set; }
    }
}