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

            return View(grants);
        }
    }
}