using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;

namespace TrellYeahCapstone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class HomeIndexViewModel
        {
            public string? ChairDepartmentName { get; set; }
            public List<ApplicationUser> DepartmentUsers { get; set; } = new();
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new HomeIndexViewModel();

            if (user != null && User.IsInRole("DeptChair"))
            {
                var department = await _context.Departments.FirstOrDefaultAsync(d => d.ChairUserId == user.Id); //Dallen - Get department that this user is chair of
                if (department != null)
                {
                    model.ChairDepartmentName = department.Name;
                    model.DepartmentUsers = await _userManager.Users.Where(u => u.DepartmentId == department.Id && u.Id != user.Id).ToListAsync(); //Dallen - Get list of all members of department that are not the chair
                }
            }

            return View(model);
        }

        //public IActionResult Privacy() //Dallen - No longer use the privacy scaffolding page
        //{
        //    return View();
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
