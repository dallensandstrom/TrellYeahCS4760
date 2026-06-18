namespace TrellYeahCapstone.Models
{
    public class GrantReviewCriterionScoreViewModel
    {
        public int RubricCriterionId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int MaximumScore { get; set; }

        public int? SelectedScore { get; set; }

        public List<RubricRatingSuggestion> RatingSuggestions { get; set; } = new();
    }
}
