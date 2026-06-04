using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Models
{
    public class BudgetWorksheetViewModel
    {
        public int GrantId { get; set; }
        public string GrantTitle { get; set; } = string.Empty;
        public List<BudgetItem> BudgetItems { get; set; } = new();
    }
}
