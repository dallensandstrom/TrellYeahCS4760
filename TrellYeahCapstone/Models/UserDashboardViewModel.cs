using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Models
{
    public class UserDashboardViewModel
    {
        public List<Grant> SavedGrants { get; set; } = new();
        public List<Grant> SubmittedGrants { get; set; } = new();
        public List<Grant> AcceptedGrants { get; set; } = new();
        public List<Grant> RejectedGrants { get; set; } = new();
        public List<(Grant Grant, GrantReport Report)> ReportedGrants { get; set; } = new();
    }
}
