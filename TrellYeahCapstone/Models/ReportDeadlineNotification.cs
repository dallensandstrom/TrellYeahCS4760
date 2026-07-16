namespace TrellYeahCapstone.Models
{
    public class ReportDeadlineNotification //Dallen - Used to pass information about report deadlines to the view for display in the notification area of the home page.
    {
        public int GrantId { get; set; }
        public string GrantTitle { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysRemaining { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AlertClass { get; set; } = "alert-info";
    }
}
