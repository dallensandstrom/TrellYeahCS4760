using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;
using TrellYeahCS4760.Models;

namespace TrellYeahCapstone.Controllers
{
    [Authorize]
    public class GrantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GrantsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create()
        {
            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;

            var grant = new Grant
            {
                ProjectDirectorUserId = currentUserId,
                PrincipalInvestigatorUserId = currentUserId,
                UserOptions = await GetUserOptionsAsync()
            };

            return View(grant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Grant grant)
        {
            if (grant.BenefitsMultipleDepartments &&
                grant.NumberOfDepartmentsBenefited == null)
            {
                ModelState.AddModelError(
                    nameof(grant.NumberOfDepartmentsBenefited),
                    "Enter how many departments will benefit.");
            }

            if (!ModelState.IsValid)
            {
                grant.UserOptions = await GetUserOptionsAsync();
                return View(grant);
            }

            grant.UserId = _userManager.GetUserId(User);
            grant.SubmittedAt = DateTime.Now;

            _context.Grants.Add(grant);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Grant request submitted successfully!";

            return RedirectToAction("Index", "UserDashboard");
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCollegeAndDepartment(string userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Json(new
                {
                    college = "Not assigned",
                    department = "Not assigned"
                });
            }

            var college = await _context.Colleges
                .Where(c => c.Id == user.CollegeId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            var department = await _context.Departments
                .Where(d => d.Id == user.DepartmentId)
                .Select(d => d.Name)
                .FirstOrDefaultAsync();

            return Json(new
            {
                college = college ?? "Not assigned",
                department = department ?? "Not assigned"
            });
        }

        private async Task<List<SelectListItem>> GetUserOptionsAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Email)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email ?? "Unknown user"
                })
                .ToListAsync();
        }
    }
}