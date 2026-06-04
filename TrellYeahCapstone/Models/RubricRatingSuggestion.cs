using System.ComponentModel.DataAnnotations;

namespace TrellYeahCapstone.Models
{
    public class RubricRatingSuggestion
    {
        public int RubricRatingSuggestionId { get; set; }

        [Required]
        public int RubricCriterionId { get; set; }

        public RubricCriterion? RubricCriterion { get; set; }

        [Range(0, 1000)]
        public int Score { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
