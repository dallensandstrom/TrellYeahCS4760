using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;
using TrellYeahCapstone.Services;

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
            public string? DeanCollegeName { get; set; }
            public List<ApplicationUser> CollegeMembers { get; set; } = new();
            public List<ReportDeadlineNotification> ReportNotifications { get; set; } = new();

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
            else if (user != null && User.IsInRole("CollegeDean"))
            {
                var college = await _context.Colleges.FirstOrDefaultAsync(c => c.DeanUserId == user.Id); //Dallen - Get college that this user is dean of
                if (college != null)
                {
                    model.DeanCollegeName = college.Name;
                    model.CollegeMembers = await _userManager.Users.Where(u => u.CollegeId == college.Id && u.Id != user.Id).ToListAsync(); //Dallen - Get list of all members of the college that are not the dean
                }
            }
            //Dallen - Get list of grants that are approved or accepted and have a report due date
            if (user != null)
            {
                //Dallen - Get list of grants that are approved or accepted and have a report due date, and do not have a report submitted yet
                var grantsNeedingReports = await _context.Grants.Where(g => g.UserId == user.Id && g.ReportDueDate != null && (g.Status == "Approved by ARCC" || g.Status == "Accepted") && !_context.GrantReports.Any(r => r.GrantId == g.GrantId)).ToListAsync();

                //Dallen - Create a list of report notifications for the grants that need reports, ordered by due date
                model.ReportNotifications = grantsNeedingReports.Select(g => ReportDeadlineNotificationService.CreateNotification(g, DateTime.Today)).Where(notification => notification != null).Select(notification => notification!).OrderBy(notification => notification.DueDate).ToList();
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
