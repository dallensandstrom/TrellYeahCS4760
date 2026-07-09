using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Tests;

public class GrantDatabaseTests
{
    [Fact]
    public async Task Grant_CanBeSavedAndQueriedFromDatabase()
    {
        var testGrantTitle = $"Database Test Grant {Guid.NewGuid():N}";

        DbContextOptions<ApplicationDbContext> options = new DbContextOptions<ApplicationDbContext>();
        DbContextOptionsBuilder builder = new DbContextOptionsBuilder(options);
        SqlServerDbContextOptionsExtensions.UseSqlServer(builder, "Server=(localdb)\\mssqllocaldb;Database=aspnet-TrellYeahCapstone-727108a2-0106-47d2-8a96-99b80a9c2edf;Trusted_Connection=True;MultipleActiveResultSets=true", null);
        var context = new ApplicationDbContext((DbContextOptions<ApplicationDbContext>)builder.Options);

        try
        {
            var grant = new Grant
            {
                Title = testGrantTitle,
                Description = "This grant exists for a database unit test.",
                Justification = "This verifies that EF can save and query a grant.",
                AccountNumber = 12345,
                ProjectDirectorUserId = "user-1",
                PrincipalInvestigatorUserId = "user-1",
                ApplicationSignature = "Test User",
                Status = "Submitted",
                SubmittedAt = DateTime.Now
            };

            context.Grants.Add(grant);
            await context.SaveChangesAsync();

            var savedGrant = await context.Grants
                .FirstOrDefaultAsync(g => g.Title == testGrantTitle);

            Assert.NotNull(savedGrant);
            Assert.True(savedGrant.GrantId > 0);
            Assert.Equal("Submitted", savedGrant.Status);
            Assert.Equal(testGrantTitle, savedGrant.Title);
        }
        finally
        {
            var testGrants = await context.Grants
                .Where(g => g.Title == testGrantTitle)
                .ToListAsync();

            context.Grants.RemoveRange(testGrants);
            await context.SaveChangesAsync();
            await context.DisposeAsync();
        }
    }
}
