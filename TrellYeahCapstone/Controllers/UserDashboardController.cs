using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;
using TrellYeahCS4760.Models;

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
        public async Task<IActionResult> QuickSummary()
        {
            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .AsNoTracking()
                .ToListAsync();

            var users = await _context.Users
                .AsNoTracking()
                .ToDictionaryAsync(user => user.Id);

            var colleges = await _context.Colleges
                .AsNoTracking()
                .ToDictionaryAsync(
                    college => college.Id,
                    college => college.Name);

            var departments = await _context.Departments
                .AsNoTracking()
                .ToDictionaryAsync(
                    department => department.Id,
                    department => department);

            var submittedReportGrantIds = await _context.GrantReports
                .AsNoTracking()
                .Select(report => report.GrantId)
                .Distinct()
                .ToListAsync();

            var submittedReportSet = submittedReportGrantIds.ToHashSet();

            var awardedStatuses = new[]
            {
        "Approved by ARCC",
        "Accepted"
    };

            var awardedGrants = grants
                .Where(grant => awardedStatuses.Contains(grant.Status))
                .ToList();

            string GetCollegeName(Grant grant)
            {
                if (!users.TryGetValue(
                        grant.ProjectDirectorUserId,
                        out var projectDirector) ||
                    !projectDirector.CollegeId.HasValue)
                {
                    return "Unassigned college";
                }

                return colleges.GetValueOrDefault(
                    projectDirector.CollegeId.Value,
                    "Unassigned college");
            }

            string GetDepartmentName(Grant grant)
            {
                if (!users.TryGetValue(
                        grant.ProjectDirectorUserId,
                        out var projectDirector) ||
                    !projectDirector.DepartmentId.HasValue)
                {
                    return "Unassigned department";
                }

                return departments.TryGetValue(
                    projectDirector.DepartmentId.Value,
                    out var department)
                    ? department.Name
                    : "Unassigned department";
            }

            var totalAwarded = awardedGrants
                .Sum(grant => grant.AllocatedAmount ?? 0m);

            var reportsSubmitted = awardedGrants.Count(
                grant => submittedReportSet.Contains(grant.GrantId));

            var model = new ArccChairSummaryViewModel
            {
                TotalApplications = grants.Count(
                    grant => grant.Status != "In Progress"),

                AwardedGrantCount = awardedGrants.Count,

                TotalAwarded = totalAwarded,

                TotalMatchingFunds = awardedGrants.Sum(grant =>
                    grant.BudgetItems.Sum(item =>
                        item.CollegeAmount +
                        item.DepartmentAmount +
                        item.OtherAmount)),

                StudentsBenefited = awardedGrants.Sum(
                    grant => grant.WeberStateStudentsBenefited),

                ReportsSubmitted = reportsSubmitted,

                ReportsOutstanding =
                    awardedGrants.Count - reportsSubmitted,

                AverageAward = awardedGrants.Count == 0
                    ? 0m
                    : totalAwarded / awardedGrants.Count,

                AwardsByCollege = awardedGrants
                    .GroupBy(GetCollegeName)
                    .Select(group => new ArccSummaryAmountRow
                    {
                        Label = group.Key,

                        Amount = group.Sum(
                            grant => grant.AllocatedAmount ?? 0m),

                        GrantCount = group.Count()
                    })
                    .OrderByDescending(row => row.Amount)
                    .ToList(),

                AwardsByDepartment = awardedGrants
                    .GroupBy(GetDepartmentName)
                    .Select(group => new ArccSummaryAmountRow
                    {
                        Label = group.Key,

                        Amount = group.Sum(
                            grant => grant.AllocatedAmount ?? 0m),

                        GrantCount = group.Count()
                    })
                    .OrderByDescending(row => row.Amount)
                    .Take(10)
                    .ToList(),

                FundingSources = new List<ArccSummaryAmountRow>
        {
            new()
            {
                Label = "ARCC awards",
                Amount = awardedGrants.Sum(
                    grant => grant.AllocatedAmount ?? 0m)
            },

            new()
            {
                Label = "College contributions",
                Amount = awardedGrants.Sum(grant =>
                    grant.BudgetItems.Sum(
                        item => item.CollegeAmount))
            },

            new()
            {
                Label = "Department contributions",
                Amount = awardedGrants.Sum(grant =>
                    grant.BudgetItems.Sum(
                        item => item.DepartmentAmount))
            },

            new()
            {
                Label = "Other contributions",
                Amount = awardedGrants.Sum(grant =>
                    grant.BudgetItems.Sum(
                        item => item.OtherAmount))
            }
        }
                .Where(row => row.Amount > 0)
                .ToList(),

                ApplicationStatuses = grants
                    .Where(grant => grant.Status != "In Progress")
                    .GroupBy(grant => grant.Status)
                    .Select(group => new ArccSummaryCountRow
                    {
                        Label = group.Key,
                        Count = group.Count()
                    })
                    .OrderByDescending(row => row.Count)
                    .ToList(),

                ReportsByStatus = new List<ArccSummaryCountRow>
        {
            new()
            {
                Label = "Submitted",
                Count = reportsSubmitted
            },

            new()
            {
                Label = "Outstanding",
                Count = awardedGrants.Count - reportsSubmitted
            }
        },

                RecentAwards = awardedGrants
                    .OrderByDescending(grant =>
                        grant.AwardDate ??
                        grant.SubmittedAt ??
                        DateTime.MinValue)
                    .Take(8)
                    .Select(grant => new ArccSummaryGrantRow
                    {
                        Title = grant.Title,
                        College = GetCollegeName(grant),
                        Department = GetDepartmentName(grant),
                        AwardedAmount = grant.AllocatedAmount ?? 0m,
                        StudentsBenefited =
                            grant.WeberStateStudentsBenefited,
                        AwardDate = grant.AwardDate,
                        ReportSubmitted =
                            submittedReportSet.Contains(grant.GrantId)
                    })
                    .ToList()
            };

            return View(model);
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

            var principalInvestigators = await _context.Users
                .Where(user => piIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id);

            var averageScores = await BuildAverageScoreLookupAsync(grantIds);

            return submittedGrants
                .Select(g =>
                {
                    averageScores.TryGetValue(g.GrantId, out var score);
                    principalInvestigators.TryGetValue(g.PrincipalInvestigatorUserId, out var principalInvestigator);

                    return new AllocationGrantSummaryViewModel
                    {
                        GrantId = g.GrantId,
                        Title = g.Title,
                        PrincipalInvestigatorName = principalInvestigator == null
                            ? "Unknown user"
                            : GetUserDisplayName(principalInvestigator),
                        PrincipalInvestigatorAccountNumber = principalInvestigator?.AccountNumber ?? 0,
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

            await ClearSubmittedGrantAllocationsAsync();
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
                await ClearSubmittedGrantAllocationsAsync();
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
            var latestAllocation = await _context.GrantAllocations
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestAllocation == null)
            {
                TempData["ErrorMessage"] = "No allocation round has been started. Please set the available amount first.";
                return RedirectToAction(nameof(Allocation));
            }

            var available = latestAllocation.CurrentRoundAmount;
            var criteria = await _context.AllocationCriteria.ToListAsync();

            var summaries = await BuildGrantAllocationSummariesAsync(ArccReviewStatuses);
            var grantIds = summaries.Select(s => s.GrantId).ToList();

            var grants = await _context.Grants
                .Where(g => grantIds.Contains(g.GrantId))
                .Include(g => g.BudgetItems)
                .ToListAsync();

            // Calculate initial allocations from criteria
            var allocationMap = new Dictionary<int, decimal?>();
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
                        // Edge case 3: never allocate more than what was requested
                        allocated = Math.Min(allocated.Value, requested);
                    }
                }

                allocationMap[grant.GrantId] = allocated;
            }

            var totalAllocated = allocationMap.Values.Sum(v => v ?? 0);

            // Edge case 1: criteria would spend more than is available
            if (totalAllocated > available)
            {
                TempData["ErrorMessage"] = $"Cannot apply: criteria would allocate {totalAllocated:C} but only {available:C} is available. Reduce allocation percentages and try again.";
                return RedirectToAction(nameof(Allocation));
            }

            // Edge case 2: 5% gap rule — if less than 95% of funds would be used,
            // top up grants (highest score first) toward their full request
            var target = Math.Round(available * 0.95m, 2);
            if (totalAllocated < target)
            {
                var remaining = target - totalAllocated;

                var orderedGrants = grants
                    .Select(g => (Grant: g, Summary: summaries.First(s => s.GrantId == g.GrantId)))
                    .Where(x => (allocationMap[x.Grant.GrantId] ?? 0) > 0)
                    .OrderByDescending(x => x.Summary.AverageScorePercentage ?? 0)
                    .ToList();

                foreach (var (grant, _) in orderedGrants)
                {
                    if (remaining <= 0) break;
                    var requested = grant.BudgetItems.Sum(b => b.ARCCAmount);
                    var current = allocationMap[grant.GrantId] ?? 0;
                    // Edge case 3: cap at requested amount
                    var canAdd = requested - current;
                    var toAdd = Math.Round(Math.Min(canAdd, remaining), 2);
                    if (toAdd > 0)
                    {
                        allocationMap[grant.GrantId] = current + toAdd;
                        remaining -= toAdd;
                    }
                }

                totalAllocated = allocationMap.Values.Sum(v => v ?? 0);

                // Safety net: if the fill-up somehow exceeded available (shouldn't happen)
                if (totalAllocated > available)
                {
                    TempData["ErrorMessage"] = $"Cannot apply: total would exceed available {available:C}. Please try again.";
                    return RedirectToAction(nameof(Allocation));
                }

                if (totalAllocated < target)
                {
                    // Edge case 3 prevented closing the gap — warn the user
                    TempData["WarningMessage"] = $"Gap exceeds 5%: allocated {totalAllocated:C} of {available:C} available. All eligible requests are fully funded but there are insufficient applications to close the gap.";
                }
            }

            foreach (var grant in grants)
            {
                grant.AllocatedAmount = allocationMap[grant.GrantId];
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Allocation criteria applied. Total allocated: {totalAllocated:C} of {available:C} available.";
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

            var principalInvestigators = await _context.Users
                .Where(user => piIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id);

            var spreadsheet = new StringBuilder();
            spreadsheet.AppendLine("Title,Principal Investigator,Account Number,Allocated Money");

            foreach (var grant in grants)
            {
                principalInvestigators.TryGetValue(grant.PrincipalInvestigatorUserId, out var principalInvestigator);
                var principalInvestigatorName = principalInvestigator == null
                    ? "Unknown user"
                    : GetUserDisplayName(principalInvestigator);
                var accountNumber = principalInvestigator?.AccountNumber.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                var allocatedAmount = grant.AllocatedAmount.GetValueOrDefault()
                    .ToString("C2", CultureInfo.GetCultureInfo("en-US"));

                spreadsheet.AppendLine(string.Join(",",
                    EscapeCsvValue(grant.Title),
                    EscapeCsvValue(principalInvestigatorName),
                    EscapeCsvValue(accountNumber),
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

            string? reportFilePath = null;

            if (model.ReportFile != null && model.ReportFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "grant-reports");

                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ReportFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ReportFile.CopyToAsync(stream);

                reportFilePath = $"/uploads/grant-reports/{uniqueFileName}";
            }

            var report = new GrantReport
            {
                GrantId = grant.GrantId,
                SubmissionDate = DateTime.Now,
                ProjectSummary = model.ProjectSummary,
                CurrentProgress = model.CurrentProgress,
                NextSteps = model.NextSteps,
                Budget = model.Budget,
                ReportFilePath = reportFilePath
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

        private async Task ClearSubmittedGrantAllocationsAsync()
        {
            var grants = await _context.Grants
                .Where(grant => ArccReviewStatuses.Contains(grant.Status))
                .ToListAsync();

            foreach (var grant in grants)
            {
                grant.AllocatedAmount = null;
            }
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
