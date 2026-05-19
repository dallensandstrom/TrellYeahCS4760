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
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
                    .ToDictionaryAsync(user => user.Id, user => user.Email ?? user.UserName ?? user.Id)
            };

            return View(viewModel);
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
    }
}
