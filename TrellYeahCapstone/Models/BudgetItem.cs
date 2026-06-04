using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrellYeahCS4760.Models
{
    public class BudgetItem
    {
        public int BudgetItemId { get; set; }

        public int GrantId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Type")]
        public string ItemType { get; set; } = "Hardware"; // "Hardware" or "Software"

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "ARCC Amount")]
        public decimal ARCCAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "College Amount")]
        public decimal CollegeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "Department Amount")]
        public decimal DepartmentAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "Other Amount")]
        public decimal OtherAmount { get; set; }

        [StringLength(200)]
        [Display(Name = "Other Source (specify)")]
        public string? OtherSource { get; set; }

        [ForeignKey("GrantId")]
        public Grant? Grant { get; set; }
    }
}
