using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrellYeahCapstone.Models
{
    public class AddDepartmentViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Department name")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "College")]
        public int CollegeId { get; set; }

        [Required]
        [Display(Name = "Department chair")]
        public string ChairUserId { get; set; } = string.Empty;

        public List<SelectListItem> Colleges { get; set; } = new();

        public List<SelectListItem> Users { get; set; } = new();
    }
}
