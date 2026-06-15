using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Models
{
    public class GrantReviewDetailsViewModel
    {
        public Grant Grant { get; set; } = new();

        public string ProjectDirectorName { get; set; } = string.Empty;

        public string PrincipalInvestigatorName { get; set; } = string.Empty;

        public decimal MoneyRequestedFromArcc { get; set; }

        public List<GrantFileLinkViewModel> FileLinks { get; set; } = new();

        public List<GrantReviewCriterionScoreViewModel> RubricScores { get; set; } = new();

        public int SelectedScoreTotal => RubricScores.Sum(score => score.SelectedScore ?? 0);

        public int PossibleScoreTotal => RubricScores.Sum(score => score.MaximumScore);

        public decimal ScorePercentage => PossibleScoreTotal == 0
            ? 0
            : Math.Round((decimal)SelectedScoreTotal / PossibleScoreTotal * 100, 2);
    }
}
