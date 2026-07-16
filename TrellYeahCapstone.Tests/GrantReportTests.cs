using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;
using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Tests;

public class GrantReportTests
{
    private static ApplicationDbContext CreateContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptions<ApplicationDbContext>();
        DbContextOptionsBuilder builder = new DbContextOptionsBuilder(options);
        SqlServerDbContextOptionsExtensions.UseSqlServer(builder, "Server=(localdb)\\mssqllocaldb;Database=aspnet-TrellYeahCapstone-727108a2-0106-47d2-8a96-99b80a9c2edf;Trusted_Connection=True;MultipleActiveResultSets=true", null);
        return new ApplicationDbContext((DbContextOptions<ApplicationDbContext>)builder.Options);
    }

    [Fact]
    public async Task GrantReport_CanBeSavedAndLinkedToGrant()
    {
        var testGrantTitle = $"Report Test Grant {Guid.NewGuid():N}";
        var context = CreateContext();

        try
        {
            var grant = new Grant
            {
                Title = testGrantTitle,
                Description = "Test description",
                Justification = "Test justification",
                AccountNumber = 99999,
                ProjectDirectorUserId = "user-1",
                PrincipalInvestigatorUserId = "user-1",
                ApplicationSignature = "Test User",
                Status = "Accepted",
                SubmittedAt = DateTime.Now
            };
            context.Grants.Add(grant);
            await context.SaveChangesAsync();

            var report = new GrantReport
            {
                GrantId = grant.GrantId,
                SubmissionDate = DateTime.Now,
                ProjectSummary = "Summary",
                CurrentProgress = "Progress",
                NextSteps = "Next steps",
                Budget = "Budget details"
            };
            context.GrantReports.Add(report);
            await context.SaveChangesAsync();

            var savedReport = await context.GrantReports
                .FirstOrDefaultAsync(r => r.GrantId == grant.GrantId);

            Assert.NotNull(savedReport);
            Assert.Equal(grant.GrantId, savedReport.GrantId);
        }
        finally
        {
            var reports = await context.GrantReports
                .Where(r => r.Grant != null && r.Grant.Title == testGrantTitle)
                .ToListAsync();
            context.GrantReports.RemoveRange(reports);

            var grants = await context.Grants.Where(g => g.Title == testGrantTitle).ToListAsync();
            context.Grants.RemoveRange(grants);

            await context.SaveChangesAsync();
            await context.DisposeAsync();
        }
    }

    [Fact]
    public async Task GrantReport_SubmissionDate_IsStoredAccurately()
    {
        var testGrantTitle = $"Timestamp Test Grant {Guid.NewGuid():N}";
        var expectedDate = new DateTime(2025, 6, 15, 10, 30, 0);
        var context = CreateContext();

        try
        {
            var grant = new Grant
            {
                Title = testGrantTitle,
                Description = "Test description",
                Justification = "Test justification",
                AccountNumber = 99998,
                ProjectDirectorUserId = "user-1",
                PrincipalInvestigatorUserId = "user-1",
                ApplicationSignature = "Test User",
                Status = "Accepted",
                SubmittedAt = DateTime.Now
            };
            context.Grants.Add(grant);
            await context.SaveChangesAsync();

            var report = new GrantReport
            {
                GrantId = grant.GrantId,
                SubmissionDate = expectedDate,
                ProjectSummary = "Summary",
                CurrentProgress = "Progress",
                NextSteps = "Next steps",
                Budget = "Budget details"
            };
            context.GrantReports.Add(report);
            await context.SaveChangesAsync();

            var savedReport = await context.GrantReports
                .FirstOrDefaultAsync(r => r.GrantId == grant.GrantId);

            Assert.NotNull(savedReport);
            Assert.Equal(expectedDate, savedReport.SubmissionDate);
        }
        finally
        {
            var reports = await context.GrantReports
                .Where(r => r.Grant != null && r.Grant.Title == testGrantTitle)
                .ToListAsync();
            context.GrantReports.RemoveRange(reports);

            var grants = await context.Grants.Where(g => g.Title == testGrantTitle).ToListAsync();
            context.Grants.RemoveRange(grants);

            await context.SaveChangesAsync();
            await context.DisposeAsync();
        }
    }
}
