using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrellYeahCapstone.Models
{
    public class AddCollegeViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "College name")]
        public string CollegeName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Dean")]
        public string DeanUserId { get; set; } = string.Empty;

        public List<SelectListItem> Users { get; set; } = new();
    }
}
