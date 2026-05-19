namespace TrellYeahCapstone.Models
{
    public class AdminIndexViewModel
    {
        public List<College> Colleges { get; set; } = new();

        public List<Department> Departments { get; set; } = new();

        public Dictionary<string, string> UserEmails { get; set; } = new();
    }
}
