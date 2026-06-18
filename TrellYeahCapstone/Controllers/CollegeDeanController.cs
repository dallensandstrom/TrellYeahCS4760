using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;

namespace TrellYeahCapstone.Controllers
{
    [Authorize(Roles = "CollegeDean")]
    public class CollegeDeanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CollegeDeanController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var dean = await _userManager.GetUserAsync(User);
            if (dean == null) return Unauthorized();

            var deanCollegeId = dean.CollegeId;
            if (deanCollegeId == null)
            {
                var college = await _context.Colleges.FirstOrDefaultAsync(c => c.DeanUserId == dean.Id);
                deanCollegeId = college?.Id;
            }

            if (deanCollegeId == null) return View(new List<GrantReviewSummaryViewModel>());

            var deanStatuses = new[] { "Approved by Department Chair", "Approved by Dean", "Rejected by Dean" };

            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Where(g =>
                    deanStatuses.Contains(g.Status) &&
                    g.BudgetItems.Any(b => b.CollegeAmount > 0) &&
                    _context.Users.Any(u => u.Id == g.ProjectDirectorUserId && u.CollegeId == deanCollegeId))
                .OrderByDescending(g => g.SubmittedAt)
                .ToListAsync();

            var piIds = grants.Select(g => g.PrincipalInvestigatorUserId).Distinct().ToList();
            var piNames = await _context.Users
                .Where(u => piIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => GetUserDisplayName(u));

            var model = grants.Select(g => new GrantReviewSummaryViewModel
            {
                GrantId = g.GrantId,
                Title = g.Title,
                PrincipalInvestigatorName = piNames.GetValueOrDefault(g.PrincipalInvestigatorUserId, "Unknown user"),
                MoneyRequestedFromCollege = g.BudgetItems.Sum(b => b.CollegeAmount),
                SubmittedAt = g.SubmittedAt,
                Status = g.Status
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Review(int id)
        {
            var grant = await GetDeanGrantAsync(id);
            if (grant == null) return NotFound();

            var projectDirector = await _context.Users.FindAsync(grant.ProjectDirectorUserId);
            var principalInvestigator = await _context.Users.FindAsync(grant.PrincipalInvestigatorUserId);

            var model = new GrantReviewDetailsViewModel
            {
                Grant = grant,
                ProjectDirectorName = projectDirector == null ? "Unknown user" : GetUserDisplayName(projectDirector),
                PrincipalInvestigatorName = principalInvestigator == null ? "Unknown user" : GetUserDisplayName(principalInvestigator),
                MoneyRequestedFromArcc = grant.BudgetItems.Sum(b => b.ARCCAmount),
                FileLinks = GetGrantFileLinks(grant)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrReject(int id, string submitAction, string? notes)
        {
            var grant = await GetDeanGrantAsync(id);
            if (grant == null) return NotFound();

            grant.DeanApprovalNotes = notes?.Trim();

            if (submitAction == "Approve")
            {
                grant.Status = "Approved by Dean";
                TempData["SuccessMessage"] = "Grant approved.";
            }
            else
            {
                grant.Status = "Rejected by Dean";
                TempData["SuccessMessage"] = "Grant rejected.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<TrellYeahCS4760.Models.Grant?> GetDeanGrantAsync(int grantId)
        {
            var dean = await _userManager.GetUserAsync(User);
            if (dean == null) return null;

            var deanCollegeId = dean.CollegeId;
            if (deanCollegeId == null)
            {
                var college = await _context.Colleges.FirstOrDefaultAsync(c => c.DeanUserId == dean.Id);
                deanCollegeId = college?.Id;
            }

            if (deanCollegeId == null) return null;

            return await _context.Grants
                .Include(g => g.BudgetItems)
                .FirstOrDefaultAsync(g =>
                    g.GrantId == grantId &&
                    g.Status == "Approved by Department Chair" &&
                    g.BudgetItems.Any(b => b.CollegeAmount > 0) &&
                    _context.Users.Any(u => u.Id == g.ProjectDirectorUserId && u.CollegeId == deanCollegeId));
        }

        private static string GetUserDisplayName(ApplicationUser user)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName)
                ? user.Email ?? user.UserName ?? user.Id
                : fullName;
        }

        private static List<GrantFileLinkViewModel> GetGrantFileLinks(TrellYeahCS4760.Models.Grant grant)
        {
            var files = new List<GrantFileLinkViewModel>();
            AddFileLink(files, "Supporting Document 1", grant.SupportingDocument1Path);
            AddFileLink(files, "Supporting Document 2", grant.SupportingDocument2Path);
            AddFileLink(files, "Supporting Document 3", grant.SupportingDocument3Path);
            AddFileLink(files, "IRB Approval File", grant.IRBApprovalFilePath);
            return files;
        }

        private static void AddFileLink(List<GrantFileLinkViewModel> files, string label, string? path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                files.Add(new GrantFileLinkViewModel { Label = label, Url = path });
        }
    }
}
