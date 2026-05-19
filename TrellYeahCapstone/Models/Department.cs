using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CollegeId { get; set; }

        public College? College { get; set; }

        [Required]
        public string ChairUserId { get; set; } = string.Empty;
    }
}
