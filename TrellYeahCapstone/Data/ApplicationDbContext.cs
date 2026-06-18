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
        public DbSet<GrantAllocation> GrantAllocations { get; set; }
        public DbSet<GrantRubricScore> GrantRubricScores { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GrantRubricScore>()
                .HasIndex(score => new { score.GrantId, score.RubricCriterionId, score.ReviewerUserId })
                .IsUnique();

            builder.Entity<GrantRubricScore>()
                .HasOne(score => score.Reviewer)
                .WithMany()
                .HasForeignKey(score => score.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
