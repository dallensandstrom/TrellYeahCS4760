using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;

namespace TrellYeahCS4760.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly string[] ArccReviewStatuses =
        [
            "Submitted",
            "Approved by Department Chair",
            "Approved by Dean"
        ];
        private static readonly string[] ArccRejectedStatuses =
        [
            "Rejected by ARCC"
        ];
        private static readonly string[] ArccAccountingStatuses =
        [
            "Approved by ARCC"
        ];

        public UserDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var grants = await _context.Grants
                .Where(g => g.UserId == currentUserId)
                .OrderByDescending(g => g.SubmittedAt ?? DateTime.MinValue)
                .ThenByDescending(g => g.GrantId)
                .ToListAsync();

            var viewModel = new UserDashboardViewModel
            {
                SavedGrants = grants
                .Where(g => g.Status == "In Progress")
                .ToList(),

                SubmittedGrants = grants
                .Where(g =>
                    g.Status == "Submitted" ||
                    g.Status == "Approved by Department Chair" ||
                    g.Status == "Approved by Dean")
                .ToList(),

                AcceptedGrants = grants
                .Where(g =>
                    g.Status == "Approved by ARCC" ||
                    g.Status == "Accepted")
                .ToList(),

                RejectedGrants = grants
                .Where(g =>
                    g.Status == "Rejected by Department Chair" ||
                    g.Status == "Rejected by Dean" ||
                    g.Status == "Rejected by ARCC" ||
                    g.Status == "Rejected")
                .ToList()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "ARCCmember,ARCCchair")]
        public async Task<IActionResult> Review()
        {
            return View(await BuildSubmittedGrantReviewListAsync());
        }

        private async Task<List<GrantReviewSummaryViewModel>> BuildSubmittedGrantReviewListAsync()
        {
            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
            var submittedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Where(g => ArccReviewStatuses.Contains(g.Status))
                .OrderByDescending(g => g.SubmittedAt ?? DateTime.MinValue)
                .ThenByDescending(g => g.GrantId)
                .ToListAsync();

            var userIds = submittedGrants
                .Select(g => g.PrincipalInvestigatorUserId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .Where(user => userIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => GetUserDisplayName(user));

            var submittedGrantIds = submittedGrants
                .Select(g => g.GrantId)
                .ToList();

            var grantIdsReviewedByCurrentUser = await _context.GrantRubricScores
                .Where(score =>
                    score.ReviewerUserId == currentUserId &&
                    submittedGrantIds.Contains(score.GrantId))
                .Select(score => score.GrantId)
                .Distinct()
                .ToListAsync();

            return submittedGrants
                .Select(g => new GrantReviewSummaryViewModel
                {
                    GrantId = g.GrantId,
                    Title = g.Title,
                    PrincipalInvestigatorName = userNames.GetValueOrDefault(
                        g.PrincipalInvestigatorUserId,
                        "Unknown user"),
                    MoneyRequestedFromArcc = g.BudgetItems.Sum(item => item.ARCCAmount),
                    SubmittedAt = g.SubmittedAt,
                    Status = g.Status,
                    HasSavedReview = grantIdsReviewedByCurrentUser.Contains(g.GrantId)
                })
                .ToList();
        }

        private static string GetUserDisplayName(ApplicationUser user)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            return string.IsNullOrWhiteSpace(fullName)
                ? user.Email ?? user.UserName ?? user.Id
                : fullName;
        }

        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> Allocation()
        {
            return View(await BuildAllocationViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> Allocation(AllocationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildAllocationViewModelAsync();
                invalidModel.CurrentRoundAmount = model.CurrentRoundAmount;
                invalidModel.PreviousRoundAmount = model.PreviousRoundAmount;
                invalidModel.CutoutPercentage = model.CutoutPercentage;
                return View(invalidModel);
            }

            _context.GrantAllocations.Add(new GrantAllocation
            {
                CurrentRoundAmount = model.CurrentRoundAmount,
                PreviousRoundAmount = model.PreviousRoundAmount,
                CutoutPercentage = model.CutoutPercentage,
                CreatedAt = DateTime.Now
            });

            var rejectedCount = await ApplyCutoffPercentageAsync(model.CutoutPercentage);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cutoff applied. {rejectedCount} reviewed grant application(s) were rejected.";
            return RedirectToAction(nameof(Allocation));
        }

        private async Task<AllocationViewModel> BuildAllocationViewModelAsync()
        {
            return new AllocationViewModel
            {
                PastAllocations = await _context.GrantAllocations
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync(),
                AllocationCriteria = await _context.AllocationCriteria
                    .OrderBy(c => c.MinScorePercentage)
                    .ToListAsync(),
                SubmittedGrants = await BuildGrantAllocationSummariesAsync(ArccReviewStatuses),
                RejectedGrants = await BuildGrantAllocationSummariesAsync(ArccRejectedStatuses),
                AccountingGrants = await BuildGrantAllocationSummariesAsync(ArccAccountingStatuses)
            };
        }

        private async Task<List<AllocationGrantSummaryViewModel>> BuildGrantAllocationSummariesAsync(string[] statuses)
        {
            var submittedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Where(g => statuses.Contains(g.Status))
                .OrderByDescending(g => g.SubmittedAt ?? DateTime.MinValue)
                .ThenByDescending(g => g.GrantId)
                .ToListAsync();

            var grantIds = submittedGrants
                .Select(g => g.GrantId)
                .ToList();

            var piIds = submittedGrants
                .Select(g => g.PrincipalInvestigatorUserId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var piNames = await _context.Users
                .Where(user => piIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => GetUserDisplayName(user));

            var averageScores = await BuildAverageScoreLookupAsync(grantIds);

            return submittedGrants
                .Select(g =>
                {
                    averageScores.TryGetValue(g.GrantId, out var score);

                    return new AllocationGrantSummaryViewModel
                    {
                        GrantId = g.GrantId,
                        Title = g.Title,
                        PrincipalInvestigatorName = piNames.GetValueOrDefault(
                            g.PrincipalInvestigatorUserId,
                            "Unknown user"),
                        MoneyRequestedFromArcc = g.BudgetItems.Sum(item => item.ARCCAmount),
                        MoneyRequestedFromOtherSources = g.BudgetItems.Sum(item =>
                            item.CollegeAmount + item.DepartmentAmount + item.OtherAmount),
                        AverageScorePercentage = score.AveragePercentage,
                        ReviewerCount = score.ReviewerCount,
                        Status = g.Status,
                        AllocatedAmount = g.AllocatedAmount,
                        ReportDueDate = g.ReportDueDate
                    };
                })
                .ToList();
        }

        private async Task<Dictionary<int, (decimal? AveragePercentage, int ReviewerCount)>> BuildAverageScoreLookupAsync(List<int> grantIds)
        {
            var averages = new Dictionary<int, (decimal? AveragePercentage, int ReviewerCount)>();
            if (!grantIds.Any())
            {
                return averages;
            }

            var possibleScoreTotal = await _context.RubricCriteria
                .SumAsync(criterion => (int?)criterion.MaximumScore) ?? 0;

            if (possibleScoreTotal == 0)
            {
                return averages;
            }

            var rubricScores = await _context.GrantRubricScores
                .Where(score => grantIds.Contains(score.GrantId))
                .ToListAsync();

            foreach (var grantScoreGroup in rubricScores.GroupBy(score => score.GrantId))
            {
                var reviewerPercentages = grantScoreGroup
                    .GroupBy(score => score.ReviewerUserId)
                    .Select(reviewerScores =>
                        reviewerScores.Sum(score => score.Score) / (decimal)possibleScoreTotal * 100)
                    .ToList();

                if (reviewerPercentages.Any())
                {
                    averages[grantScoreGroup.Key] = (
                        Math.Round(reviewerPercentages.Average(), 2),
                        reviewerPercentages.Count);
                }
            }

            return averages;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> AddCriterion(AllocationViewModel model)
        {
            if (model.NewCriterionMinScore >= model.NewCriterionMaxScore)
            {
                TempData["ErrorMessage"] = "Min score must be less than max score.";
                return RedirectToAction(nameof(Allocation));
            }

            _context.AllocationCriteria.Add(new AllocationCriterion
            {
                MinScorePercentage = model.NewCriterionMinScore,
                MaxScorePercentage = model.NewCriterionMaxScore,
                AllocationPercentage = model.NewCriterionAllocationPercentage
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Criterion added.";
            return RedirectToAction(nameof(Allocation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> DeleteCriterion(int id)
        {
            var criterion = await _context.AllocationCriteria.FindAsync(id);
            if (criterion != null)
            {
                _context.AllocationCriteria.Remove(criterion);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Criterion removed.";
            return RedirectToAction(nameof(Allocation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> ApplyCriteria()
        {
            var criteria = await _context.AllocationCriteria.ToListAsync();

            var summaries = await BuildGrantAllocationSummariesAsync(ArccReviewStatuses);
            var grantIds = summaries.Select(s => s.GrantId).ToList();

            var grants = await _context.Grants
                .Where(g => grantIds.Contains(g.GrantId))
                .Include(g => g.BudgetItems)
                .ToListAsync();

            foreach (var grant in grants)
            {
                var summary = summaries.First(s => s.GrantId == grant.GrantId);
                decimal? allocated = null;

                if (summary.AverageScorePercentage.HasValue && criteria.Any())
                {
                    var score = summary.AverageScorePercentage.Value;
                    var match = criteria.FirstOrDefault(c =>
                        score >= c.MinScorePercentage && score <= c.MaxScorePercentage);

                    if (match != null)
                    {
                        var requested = grant.BudgetItems.Sum(item => item.ARCCAmount);
                        allocated = Math.Round(requested * match.AllocationPercentage / 100, 2);
                    }
                }

                grant.AllocatedAmount = allocated;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Allocation criteria applied to submitted grants.";
            return RedirectToAction(nameof(Allocation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> FinishAllocating()
        {
            var reportDueDate = GetReportDueDate(DateTime.Today);
            var grantsToAward = await _context.Grants
                .Where(grant =>
                    ArccReviewStatuses.Contains(grant.Status) &&
                    grant.AllocatedAmount.HasValue &&
                    grant.AllocatedAmount.Value > 0)
                .ToListAsync();

            foreach (var grant in grantsToAward)
            {
                grant.Status = "Approved by ARCC";
                grant.AwardDate = DateTime.Today;
                grant.ReportDueDate = reportDueDate;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Allocation finished. {grantsToAward.Count} funded grant application(s) were approved by ARCC.";
            return RedirectToAction(nameof(Allocation));
        }

        [HttpGet]
        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> SendToAccounting()
        {
            var grants = await _context.Grants
                .Where(grant =>
                    ArccAccountingStatuses.Contains(grant.Status) &&
                    grant.AllocatedAmount.HasValue &&
                    grant.AllocatedAmount.Value > 0)
                .OrderBy(grant => grant.Title)
                .ToListAsync();

            var piIds = grants
                .Select(grant => grant.PrincipalInvestigatorUserId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var piNames = await _context.Users
                .Where(user => piIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => GetUserDisplayName(user));

            var spreadsheet = new StringBuilder();
            spreadsheet.AppendLine("Title,Principal Investigator,Allocated Money");

            foreach (var grant in grants)
            {
                var principalInvestigator = piNames.GetValueOrDefault(
                    grant.PrincipalInvestigatorUserId,
                    "Unknown user");
                var allocatedAmount = grant.AllocatedAmount.GetValueOrDefault()
                    .ToString("0.00", CultureInfo.InvariantCulture);

                spreadsheet.AppendLine(string.Join(",",
                    EscapeCsvValue(grant.Title),
                    EscapeCsvValue(principalInvestigator),
                    EscapeCsvValue(allocatedAmount)));
            }

            var fileName = $"AccountingAllocations_{DateTime.Today:yyyyMMdd}.csv";
            return File(
                Encoding.UTF8.GetBytes(spreadsheet.ToString()),
                "text/csv",
                fileName);
        }
                
        // Dallen - Submit Report should check if the grant was approved and accepted before allowing the user to submit a report on it
        [HttpGet]
        public async Task<IActionResult> SubmitReport(int grantId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var grant = await _context.Grants.FirstOrDefaultAsync(g => g.GrantId == grantId && g.UserId == currentUserId);

            if (grant == null)
            {
                return NotFound();
            }

            if (grant.Status != "Approved by ARCC" && grant.Status != "Accepted")
            {
                return Forbid();
            }

            var projectDirector = await _context.Users.FirstOrDefaultAsync(u => u.Id == grant.ProjectDirectorUserId);

            var model = new GrantReportCreateViewModel
            {
                GrantId = grant.GrantId,

                ProjectDirector = projectDirector == null ? "Unknown User" : GetUserDisplayName(projectDirector),

                ProjectTitle = grant.Title,
                AccountNumber = grant.AccountNumber,
                SubmissionDate = DateTime.Today,
                AwardDate = grant.AwardDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReport(GrantReportCreateViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);

            var grant = await _context.Grants.FirstOrDefaultAsync(g => g.GrantId == model.GrantId && g.UserId == currentUserId);

            if (grant == null)
            {
                return NotFound();
            }

            if (grant.Status != "Approved by ARCC" && grant.Status != "Accepted")
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var projectDirector = await _context.Users.FirstOrDefaultAsync(u => u.Id == grant.ProjectDirectorUserId);

                model.ProjectDirector = projectDirector == null ? "Unknown User" : GetUserDisplayName(projectDirector);

                model.ProjectTitle = grant.Title;
                model.AccountNumber = grant.AccountNumber;
                model.SubmissionDate = DateTime.Today;
                model.AwardDate = grant.AwardDate;

                return View(model);
            }

            var report = new GrantReport
            {
                GrantId = grant.GrantId,
                SubmissionDate = DateTime.Now,
                ProjectSummary = model.ProjectSummary,
                CurrentProgress = model.CurrentProgress,
                NextSteps = model.NextSteps,
                Budget = model.Budget
            };

            _context.GrantReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project report submitted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<int> ApplyCutoffPercentageAsync(decimal cutoffPercentage)
        {
            var grantSummaries = await BuildGrantAllocationSummariesAsync(ArccReviewStatuses);
            var rejectedGrantIds = grantSummaries
                .Where(grant =>
                    grant.AverageScorePercentage.HasValue &&
                    grant.AverageScorePercentage.Value < cutoffPercentage)
                .Select(grant => grant.GrantId)
                .ToList();

            if (!rejectedGrantIds.Any())
            {
                return 0;
            }

            var grantsToReject = await _context.Grants
                .Where(grant => rejectedGrantIds.Contains(grant.GrantId))
                .ToListAsync();

            foreach (var grant in grantsToReject)
            {
                grant.Status = "Rejected by ARCC";
            }

            return grantsToReject.Count;
        }

        private static DateTime GetReportDueDate(DateTime today)
        {
            var academicYearEnd = today.Date <= new DateTime(today.Year, 6, 30)
                ? new DateTime(today.Year, 6, 30)
                : new DateTime(today.Year + 1, 6, 30);

            return academicYearEnd.AddYears(1);
        }

        private static string EscapeCsvValue(string value)
        {
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}
