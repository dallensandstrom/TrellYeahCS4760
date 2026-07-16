using TrellYeahCapstone.Models;
using TrellYeahCS4760.Models;
// Dallen - This service is used to keep the logic for creating report deadline notifications separate from the home controller and view.
namespace TrellYeahCapstone.Services
{
    public static class ReportDeadlineNotificationService
    {
        public static ReportDeadlineNotification? CreateNotification(Grant grant, DateTime today)
        {
            if (!grant.ReportDueDate.HasValue)
            {
                return null;
            }

            int daysRemaining = (grant.ReportDueDate.Value.Date - today.Date).Days;

            string? message;
            string alertClass;

            if (daysRemaining < 0)
            {
                message = $"Report for {grant.Title} is overdue.";
                alertClass = "alert-danger";
            }
            else if (daysRemaining <= 1)
            {
                message = $"Report for {grant.Title} is due in {daysRemaining} day(s).";
                alertClass = "alert-danger";
            }
            else if (daysRemaining <= 7)
            {
                message = $"Report for {grant.Title} is due within one week.";
                alertClass = "alert-warning";
            }
            else if (daysRemaining <= 30)
            {
                message = $"Report for {grant.Title} is due within one month.";
                alertClass = "alert-info";
            }
            else
            {
                return null;
            }

            return new ReportDeadlineNotification
            {
                GrantId = grant.GrantId,
                GrantTitle = grant.Title,
                DueDate = grant.ReportDueDate.Value,
                DaysRemaining = daysRemaining,
                Message = message,
                AlertClass = alertClass
            };
        }
    }
}