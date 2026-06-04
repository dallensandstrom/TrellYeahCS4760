using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Models;
using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<College> Colleges { get; set; }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Grant> Grants { get; set; }
        public DbSet<BudgetItem> BudgetItems { get; set; }
        public DbSet<RubricCriterion> RubricCriteria { get; set; }
        public DbSet<RubricRatingSuggestion> RubricRatingSuggestions { get; set; }
    }
}
