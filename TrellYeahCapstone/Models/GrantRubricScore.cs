using System.ComponentModel.DataAnnotations;
using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Models
{
    public class GrantRubricScore
    {
        public int GrantRubricScoreId { get; set; }

        public int GrantId { get; set; }

        public Grant? Grant { get; set; }

        public int RubricCriterionId { get; set; }

        public RubricCriterion? RubricCriterion { get; set; }

        [Required]
        public string ReviewerUserId { get; set; } = string.Empty;

        public ApplicationUser? Reviewer { get; set; }

        [Range(0, 1000)]
        public int Score { get; set; }
    }
}
