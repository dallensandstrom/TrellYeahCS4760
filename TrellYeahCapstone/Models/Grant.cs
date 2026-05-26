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

        [NotMapped]
        public List<SelectListItem> UserOptions { get; set; } = new();
    }
}