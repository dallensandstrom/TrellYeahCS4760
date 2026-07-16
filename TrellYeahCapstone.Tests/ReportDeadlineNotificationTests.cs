using TrellYeahCapstone.Services;
using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Tests
{
    public class ReportDeadlineNotificationTests
    {
        [Fact]
        public void CreateNotification_ReturnsMonthWarning()
        {
            var today = new DateTime(2028, 6, 1);

            var grant = CreateGrant(today.AddDays(30));

            var result =
                ReportDeadlineNotificationService.CreateNotification(
                    grant,
                    today);

            Assert.NotNull(result);
            Assert.Equal(30, result.DaysRemaining);
            Assert.Contains("one month", result.Message);
        }

        [Fact]
        public void CreateNotification_ReturnsWeekWarning()
        {
            var today = new DateTime(2028, 6, 1);

            var grant = CreateGrant(today.AddDays(7));

            var result =
                ReportDeadlineNotificationService.CreateNotification(
                    grant,
                    today);

            Assert.NotNull(result);
            Assert.Equal(7, result.DaysRemaining);
            Assert.Contains("one week", result.Message);
        }

        [Fact]
        public void CreateNotification_ReturnsDayWarning()
        {
            var today = new DateTime(2028, 6, 1);

            var grant = CreateGrant(today.AddDays(1));

            var result =
                ReportDeadlineNotificationService.CreateNotification(
                    grant,
                    today);

            Assert.NotNull(result);
            Assert.Equal(1, result.DaysRemaining);
            Assert.Equal("alert-danger", result.AlertClass);
        }

        [Fact]
        public void CreateNotification_ReturnsNullWhenDeadlineIsFarAway()
        {
            var today = new DateTime(2028, 6, 1);

            var grant = CreateGrant(today.AddDays(31));

            var result =
                ReportDeadlineNotificationService.CreateNotification(grant, today);

            Assert.Null(result);
        }

        private static Grant CreateGrant(DateTime dueDate)
        {
            return new Grant
            {
                GrantId = 1,
                Title = "Test Grant",
                Description = "Test description",
                Justification = "Test justification",
                AccountNumber = 12345,
                ProjectDirectorUserId = "user-1",
                PrincipalInvestigatorUserId = "user-1",
                ApplicationSignature = "Test User",
                Status = "Approved by ARCC",
                ReportDueDate = dueDate
            };
        }
    }
}