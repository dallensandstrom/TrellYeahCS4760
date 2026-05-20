using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrellYeahCapstone.Data;
using TrellYeahCS4760.Models;

namespace TrellYeahCS4760.Controllers
{
    [Authorize]
    public class GrantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GrantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Grant grant)
        {
            if (ModelState.IsValid)
            {
                grant.SubmittedAt = DateTime.Now;

                _context.Grants.Add(grant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Grant request submitted successfully!";

                return RedirectToAction("Index", "UserDashboard");
            }

            return View(grant);
        }
    }
}