using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrellYeahCapstone.Data;

namespace TrellYeahCS4760.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}