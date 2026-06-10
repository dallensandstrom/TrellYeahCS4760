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
    }
}
