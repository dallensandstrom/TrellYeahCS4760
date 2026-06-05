using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;

namespace TrellYeahCapstone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminIndexViewModel
            {
                Colleges = await _context.Colleges
                    .OrderBy(college => college.Name)
                    .ToListAsync(),
                Departments = await _context.Departments
                    .Include(department => department.College)
                    .OrderBy(department => department.Name)
                    .ToListAsync(),
                UserEmails = await _userManager.Users
                    .ToDictionaryAsync(user => user.Id, user => user.Email ?? user.UserName ?? user.Id),
                ArccCommitteeMembers = await _userManager.Users
                    .Where(user => user.IsArccCommitteeMember)
                    .OrderBy(user => user.LastName)
                    .ThenBy(user => user.FirstName)
                    .ThenBy(user => user.Email)
                    .ToListAsync(),
                AvailableArccUsers = await _userManager.Users
                    .Where(user => !user.IsArccCommitteeMember)
                    .OrderBy(user => user.LastName)
                    .ThenBy(user => user.FirstName)
                    .ThenBy(user => user.Email)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddArccCommitteeMember(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["StatusMessage"] = "User could not be found.";
                return RedirectToAction(nameof(Index));
            }

            user.IsArccCommitteeMember = true;
            await _userManager.UpdateAsync(user);
            await _userManager.AddToRoleAsync(user, "ARCCmember");

            TempData["StatusMessage"] = $"{GetUserDisplayName(user)} was added to the ARCC committee.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveArccCommitteeMember(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["StatusMessage"] = "User could not be found.";
                return RedirectToAction(nameof(Index));
            }

            user.IsArccCommitteeMember = false;
            user.IsArccCommitteeChair = false;
            await _userManager.UpdateAsync(user);
            await _userManager.RemoveFromRoleAsync(user, "ARCCchair");
            await _userManager.RemoveFromRoleAsync(user, "ARCCmember");

            TempData["StatusMessage"] = $"{GetUserDisplayName(user)} was removed from the ARCC committee.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetArccCommitteeChair(string userId)
        {
            var selectedUser = await _userManager.FindByIdAsync(userId);

            if (selectedUser == null || !selectedUser.IsArccCommitteeMember)
            {
                TempData["StatusMessage"] = "Select an existing ARCC committee member to be chair.";
                return RedirectToAction(nameof(Index));
            }

            var currentChairs = await _userManager.Users
                .Where(user => user.IsArccCommitteeChair)
                .ToListAsync();

            foreach (var chair in currentChairs)
            {
                chair.IsArccCommitteeChair = false;
                await _userManager.UpdateAsync(chair);
                await _userManager.RemoveFromRoleAsync(chair, "ARCCchair");
            }

            selectedUser.IsArccCommitteeMember = true;
            selectedUser.IsArccCommitteeChair = true;
            await _userManager.UpdateAsync(selectedUser);
            await _userManager.AddToRoleAsync(selectedUser, "ARCCchair");
            await _userManager.AddToRoleAsync(selectedUser, "ARCCmember");

            TempData["StatusMessage"] = $"{GetUserDisplayName(selectedUser)} is now the ARCC committee chair.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddCollege()
        {
            var viewModel = new AddCollegeViewModel();
            await PopulateUsersAsync(viewModel.Users);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCollege(AddCollegeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateUsersAsync(viewModel.Users);
                return View(viewModel);
            }

            _context.Colleges.Add(new College
            {
                Name = viewModel.CollegeName,
                DeanUserId = viewModel.DeanUserId
            });

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "College added successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddDepartment()
        {
            var viewModel = new AddDepartmentViewModel();
            await PopulateCollegesAsync(viewModel.Colleges);
            await PopulateUsersAsync(viewModel.Users);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDepartment(AddDepartmentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCollegesAsync(viewModel.Colleges);
                await PopulateUsersAsync(viewModel.Users);
                return View(viewModel);
            }

            _context.Departments.Add(new Department
            {
                Name = viewModel.DepartmentName,
                CollegeId = viewModel.CollegeId,
                ChairUserId = viewModel.ChairUserId
            });

            await _context.SaveChangesAsync();

            var chair = await _userManager.FindByIdAsync(viewModel.ChairUserId);

            if (chair != null) //Dallen - If chair user exists, update their role to DeptChair
            {
                await _userManager.UpdateAsync(chair);
                await _userManager.AddToRoleAsync(chair, "DeptChair");
            }

            TempData["StatusMessage"] = "Department added successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateUsersAsync(List<SelectListItem> users)
        {
            users.Clear();

            var identityUsers = await _userManager.Users
                .OrderBy(user => user.Email)
                .ToListAsync();

            users.AddRange(identityUsers.Select(user => new SelectListItem
            {
                Value = user.Id,
                Text = user.Email ?? user.UserName ?? user.Id
            }));
        }

        private async Task PopulateCollegesAsync(List<SelectListItem> colleges)
        {
            colleges.Clear();

            var collegeList = await _context.Colleges
                .OrderBy(college => college.Name)
                .ToListAsync();

            colleges.AddRange(collegeList.Select(college => new SelectListItem
            {
                Value = college.Id.ToString(),
                Text = college.Name
            }));
        }

        private static string GetUserDisplayName(ApplicationUser user)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            return string.IsNullOrWhiteSpace(fullName)
                ? user.Email ?? user.UserName ?? user.Id
                : $"{fullName} ({user.Email ?? user.UserName})";
        }
    }
}
