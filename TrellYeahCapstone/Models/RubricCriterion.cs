using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class RubricCriterion
    {
        public int RubricCriterionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int MaximumScore { get; set; }

        public List<RubricRatingSuggestion> RatingSuggestions { get; set; } = new();
    }
}
