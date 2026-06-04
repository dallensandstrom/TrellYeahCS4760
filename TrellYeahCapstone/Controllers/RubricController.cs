using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCapstone.Models;

namespace TrellYeahCapstone.Controllers
{
    [Authorize]
    public class RubricController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RubricController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,ARCCchair")]
        public async Task<IActionResult> Index()
        {
            return View(await BuildRubricViewModelAsync());
        }

        public async Task<IActionResult> ViewRubric()
        {
            return View(await BuildRubricViewModelAsync());
        }

        private async Task<RubricIndexViewModel> BuildRubricViewModelAsync()
        {
            var viewModel = new RubricIndexViewModel
            {
                Criteria = await _context.RubricCriteria
                    .Include(criterion => criterion.RatingSuggestions)
                    .OrderBy(criterion => criterion.RubricCriterionId)
                    .ToListAsync()
            };

            foreach (var criterion in viewModel.Criteria)
            {
                criterion.RatingSuggestions = criterion.RatingSuggestions
                    .OrderByDescending(suggestion => suggestion.Score)
                    .ToList();
            }

            return viewModel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ARCCchair")]
        public async Task<IActionResult> AddCriterion(string name, string description, int maximumScore)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description) || maximumScore < 1)
            {
                TempData["StatusMessage"] = "Criterion name, description, and a positive maximum score are required.";
                return RedirectToAction(nameof(Index));
            }

            _context.RubricCriteria.Add(new RubricCriterion
            {
                Name = name.Trim(),
                Description = description.Trim(),
                MaximumScore = maximumScore
            });

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Rubric criterion added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ARCCchair")]
        public async Task<IActionResult> EditCriterion(int id, string name, string description, int maximumScore)
        {
            var criterion = await _context.RubricCriteria
                .Include(c => c.RatingSuggestions)
                .FirstOrDefaultAsync(c => c.RubricCriterionId == id);

            if (criterion == null)
            {
                TempData["StatusMessage"] = "Criterion could not be found.";
                return RedirectToAction(nameof(Index));
            }

            var highestSuggestionScore = criterion.RatingSuggestions.Any()
                ? criterion.RatingSuggestions.Max(s => s.Score)
                : 0;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(description) ||
                maximumScore < 1 ||
                maximumScore < highestSuggestionScore)
            {
                TempData["StatusMessage"] = "Criterion maximum score must be positive and at least as high as its rating suggestions.";
                return RedirectToAction(nameof(Index));
            }

            criterion.Name = name.Trim();
            criterion.Description = description.Trim();
            criterion.MaximumScore = maximumScore;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Rubric criterion updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ARCCchair")]
        public async Task<IActionResult> DeleteCriterion(int id)
        {
            var criterion = await _context.RubricCriteria
                .Include(c => c.RatingSuggestions)
                .FirstOrDefaultAsync(c => c.RubricCriterionId == id);

            if (criterion != null)
            {
                _context.RubricCriteria.Remove(criterion);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Rubric criterion removed.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ARCCchair")]
        public async Task<IActionResult> AddRatingSuggestion(int criterionId, int score, string description)
        {
            var criterion = await _context.RubricCriteria.FindAsync(criterionId);

            if (criterion == null)
            {
                TempData["StatusMessage"] = "Criterion could not be found.";
                return RedirectToAction(nameof(Index));
            }

            if (score < 0 || score > criterion.MaximumScore || string.IsNullOrWhiteSpace(description))
            {
                TempData["StatusMessage"] = $"Rating suggestion score must be between 0 and {criterion.MaximumScore}.";
                return RedirectToAction(nameof(Index));
            }

            _context.RubricRatingSuggestions.Add(new RubricRatingSuggestion
            {
                RubricCriterionId = criterionId,
                Score = score,
                Description = description.Trim()
            });

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Rating suggestion added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ARCCchair")]
        public async Task<IActionResult> EditRatingSuggestion(int id, int score, string description)
        {
            var suggestion = await _context.RubricRatingSuggestions
                .Include(s => s.RubricCriterion)
                .FirstOrDefaultAsync(s => s.RubricRatingSuggestionId == id);

            if (suggestion == null || suggestion.RubricCriterion == null)
            {
                TempData["StatusMessage"] = "Rating suggestion could not be found.";
                return RedirectToAction(nameof(Index));
            }

            if (score < 0 || score > suggestion.RubricCriterion.MaximumScore || string.IsNullOrWhiteSpace(description))
            {
                TempData["StatusMessage"] = $"Rating suggestion score must be between 0 and {suggestion.RubricCriterion.MaximumScore}.";
                return RedirectToAction(nameof(Index));
            }

            suggestion.Score = score;
            suggestion.Description = description.Trim();

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Rating suggestion updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ARCCchair")]
        public async Task<IActionResult> DeleteRatingSuggestion(int id)
        {
            var suggestion = await _context.RubricRatingSuggestions.FindAsync(id);

            if (suggestion != null)
            {
                _context.RubricRatingSuggestions.Remove(suggestion);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Rating suggestion removed.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
