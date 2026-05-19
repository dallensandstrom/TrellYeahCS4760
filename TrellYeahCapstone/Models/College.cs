using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class College
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string DeanUserId { get; set; } = string.Empty;

        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
