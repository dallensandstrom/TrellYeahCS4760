using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Tests;

public class GrantAllocationTests
{
    [Fact]
    public void FundedGrant_CanStoreReportDueDateAndApprovedStatus()
    {
        var grant = new Grant
        {
            Title = "Test Grant",
            Description = "Test description",
            Justification = "Test justification",
            ProjectDirectorUserId = "user-1",
            PrincipalInvestigatorUserId = "user-1",
            AccountNumber = 12345,
            ApplicationSignature = "Test User",
            AllocatedAmount = 100m
        };

        grant.Status = "Approved by ARCC";
        grant.ReportDueDate = new DateTime(2028, 6, 30);

        Assert.Equal("Approved by ARCC", grant.Status);
        Assert.Equal(new DateTime(2028, 6, 30), grant.ReportDueDate);
        Assert.Equal(100m, grant.AllocatedAmount);
    }
}
