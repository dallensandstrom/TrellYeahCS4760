using System.ComponentModel.DataAnnotations;

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
    }
}