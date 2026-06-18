using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;

namespace TrellYeahCapstone.Controllers
{
    [Authorize(Roles = "DeptChair")]
    public class DepartmentChairController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentChairController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var chairUser = await _userManager.GetUserAsync(User);

            if (chairUser == null)
            {
                return Unauthorized();
            }

            var chairDepartmentId = chairUser.DepartmentId;

            if (chairDepartmentId == null)
            {
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.ChairUserId == chairUser.Id);

                chairDepartmentId = department?.Id;
            }

            if (chairDepartmentId == null)
            {
                return View(new List<GrantReviewSummaryViewModel>());
            }

            var statusesToShow = new[] { "Submitted", "Approved by Department Chair", "Rejected by Department Chair" };

            var grants = await _context.Grants
                .Include(g => g.BudgetItems)
                .Where(g =>
                    statusesToShow.Contains(g.Status) &&
                    _context.Users.Any(u =>
                        u.Id == g.ProjectDirectorUserId &&
                        u.DepartmentId == chairDepartmentId))
                .OrderByDescending(g => g.SubmittedAt)
                .ToListAsync();

            var piIds = grants
                .Select(g => g.PrincipalInvestigatorUserId)
                .Distinct()
                .ToList();

            var piNames = await _context.Users
                .Where(u => piIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => GetUserDisplayName(u));

            var model = grants.Select(g => new GrantReviewSummaryViewModel
            {
                GrantId = g.GrantId,
                Title = g.Title,
                PrincipalInvestigatorName = piNames.GetValueOrDefault(
                    g.PrincipalInvestigatorUserId,
                    "Unknown user"),
                MoneyRequestedFromCollege = g.BudgetItems.Sum(b => b.CollegeAmount),
                MoneyRequestedFromDepartment = g.BudgetItems.Sum(b => b.DepartmentAmount),
                SubmittedAt = g.SubmittedAt,
                Status = g.Status
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Review(int id)
        {
            var grant = await GetDepartmentChairGrantAsync(id);

            if (grant == null)
            {
                return NotFound();
            }

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
            var grant = await GetDepartmentChairGrantAsync(id);

            if (grant == null)
            {
                return NotFound();
            }

            grant.DeptChairApprovalNotes = notes?.Trim();

            if (submitAction == "Approve")
            {
                grant.Status = "Approved by Department Chair";
                TempData["SuccessMessage"] = "Grant approved.";
            }
            else
            {
                grant.Status = "Rejected by Department Chair";
                TempData["SuccessMessage"] = "Grant rejected.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<TrellYeahCS4760.Models.Grant?> GetDepartmentChairGrantAsync(int grantId)
        {
            var chairUser = await _userManager.GetUserAsync(User);

            if (chairUser == null)
            {
                return null;
            }

            var chairDepartmentId = chairUser.DepartmentId;

            if (chairDepartmentId == null)
            {
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.ChairUserId == chairUser.Id);

                chairDepartmentId = department?.Id;
            }

            if (chairDepartmentId == null)
            {
                return null;
            }

            return await _context.Grants
                .Include(g => g.BudgetItems)
                .FirstOrDefaultAsync(g =>
                    g.GrantId == grantId &&
                    g.Status == "Submitted" &&
                    _context.Users.Any(u =>
                        u.Id == g.ProjectDirectorUserId &&
                        u.DepartmentId == chairDepartmentId));
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
            {
                files.Add(new GrantFileLinkViewModel
                {
                    Label = label,
                    Url = path
                });
            }
        }
    }
}