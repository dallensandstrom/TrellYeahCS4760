using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;

namespace TrellYeahCS4760.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

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
                UserGrants = grants
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
            var submittedGrants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Where(g =>
                    g.Status == "Submitted" ||
                    g.Status == "Approved by Department Chair" ||
                    g.Status == "Approved by Dean")
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
                    Status = g.Status
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
            var model = new AllocationViewModel
            {
                PastAllocations = await _context.GrantAllocations
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ARCCchair")]
        public async Task<IActionResult> Allocation(AllocationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.GrantAllocations.Add(new GrantAllocation
            {
                CurrentRoundAmount = model.CurrentRoundAmount,
                PreviousRoundAmount = model.PreviousRoundAmount,
                CutoutPercentage = model.CutoutPercentage,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Allocation submitted successfully!";
            return RedirectToAction(nameof(Allocation));
        }
    }
}
