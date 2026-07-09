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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

        public GrantsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Create()
        {
            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
            var user = await _userManager.GetUserAsync(User);

            var grant = new Grant
            {
                ProjectDirectorUserId = currentUserId,
                PrincipalInvestigatorUserId = currentUserId,
                UserOptions = await GetUserOptionsAsync(),
                AccountNumber = user?.AccountNumber ?? 0
            };

            return View(grant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Grant grant, string submitAction)
        {
            var isSubmitting = submitAction == "Submit";

            var currentUserId = _userManager.GetUserId(User) ?? string.Empty;

            grant.PrincipalInvestigatorUserId = currentUserId;
            ModelState.Remove(nameof(grant.PrincipalInvestigatorUserId));

            if (!isSubmitting)
            {
                ModelState.Remove(nameof(grant.Title));
                ModelState.Remove(nameof(grant.Description));
                ModelState.Remove(nameof(grant.Justification));
                ModelState.Remove(nameof(grant.AccountNumber));
                ModelState.Remove(nameof(grant.ProjectDirectorUserId));
                ModelState.Remove(nameof(grant.PrincipalInvestigatorUserId));

                grant.Title = string.IsNullOrWhiteSpace(grant.Title)
                    ? "Untitled Grant Application"
                    : grant.Title;

                grant.Description ??= string.Empty;
                grant.Justification ??= string.Empty;
                var currentUser = await _userManager.GetUserAsync(User);
                grant.AccountNumber = currentUser?.AccountNumber ?? 0;

                grant.ProjectDirectorUserId = string.IsNullOrWhiteSpace(grant.ProjectDirectorUserId)
                    ? currentUserId
                    : grant.ProjectDirectorUserId;

                grant.PrincipalInvestigatorUserId = currentUserId;
            }

            if (isSubmitting && grant.BenefitsMultipleDepartments && grant.NumberOfDepartmentsBenefited == null)
            {
                ModelState.AddModelError(
                    nameof(grant.NumberOfDepartmentsBenefited),
                    "Enter how many departments will benefit.");
            }

            // Validate IRB file requirement when using human subjects
            if (isSubmitting && grant.UsesHumanSubjects && grant.IRBApprovalFile == null)
            {
                ModelState.AddModelError(
                    nameof(grant.IRBApprovalFile),
                    "IRB Approval File is required when the project uses human subjects.");
            }

            // Validate file sizes and extensions
            var fileValidationErrors = ValidateUploadedFiles(grant);
            foreach (var error in fileValidationErrors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            if (isSubmitting) //Dallen - Make agreement checkboxes properly required
            {
                if (!grant.AgreementOne)
                    ModelState.AddModelError(nameof(grant.AgreementOne), "Agreement Required.");

                if (!grant.AgreementTwo)
                    ModelState.AddModelError(nameof(grant.AgreementTwo), "Agreement Required.");

                if (!grant.AgreementThree)
                    ModelState.AddModelError(nameof(grant.AgreementThree), "Agreement Required.");

                if (!grant.AgreementFour)
                    ModelState.AddModelError(nameof(grant.AgreementFour), "Agreement Required.");
            }

            if (!ModelState.IsValid)
            {
                grant.UserOptions = await GetUserOptionsAsync();
                return View(grant);
            }

            try
            {
                // Process file uploads
                await ProcessFileUploads(grant);

                grant.UserId = _userManager.GetUserId(User);

                grant.Status = isSubmitting ? "Submitted" : "In Progress";
                grant.SubmittedAt = isSubmitting ? DateTime.Now : null;

                var currentUser = await _userManager.GetUserAsync(User);
                grant.AccountNumber = currentUser?.AccountNumber ?? grant.AccountNumber;

                _context.Grants.Add(grant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = isSubmitting
                    ? "Grant request submitted successfully!"
                    : "Grant application saved. You can come back and finish it later.";

                return RedirectToAction("Index", "UserDashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while processing your request: {ex.Message}");
                grant.UserOptions = await GetUserOptionsAsync();
                return View(grant);
            }
        }

        private Dictionary<string, string> ValidateUploadedFiles(Grant grant)
        {
            var errors = new Dictionary<string, string>();

            var files = new[]
            {
                (nameof(grant.SupportingDocument1), grant.SupportingDocument1),
                (nameof(grant.SupportingDocument2), grant.SupportingDocument2),
                (nameof(grant.SupportingDocument3), grant.SupportingDocument3),
                (nameof(grant.IRBApprovalFile), grant.IRBApprovalFile)
            };

            foreach (var (fieldName, file) in files)
            {
                if (file != null)
                {
                    if (file.Length > MaxFileSize)
                    {
                        errors[fieldName] = $"File is too large. Maximum size is 5MB.";
                    }

                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!AllowedExtensions.Contains(extension))
                    {
                        errors[fieldName] = $"File type not allowed. Allowed types: PDF, Word, Excel.";
                    }
                }
            }

            return errors;
        }

        private async Task ProcessFileUploads(Grant grant)
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // We'll use the GrantId in the path, but since the grant isn't saved yet, we'll use a temporary naming strategy
            var files = new[]
            {
                (field: nameof(grant.SupportingDocument1), file: grant.SupportingDocument1, pathProperty: nameof(grant.SupportingDocument1Path)),
                (field: nameof(grant.SupportingDocument2), file: grant.SupportingDocument2, pathProperty: nameof(grant.SupportingDocument2Path)),
                (field: nameof(grant.SupportingDocument3), file: grant.SupportingDocument3, pathProperty: nameof(grant.SupportingDocument3Path)),
                (field: nameof(grant.IRBApprovalFile), file: grant.IRBApprovalFile, pathProperty: nameof(grant.IRBApprovalFilePath))
            };

            foreach (var (field, file, pathProperty) in files)
            {
                if (file != null)
                {
                    // Generate unique filename with timestamp
                    var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Store relative path for database
                    var relativePath = $"/uploads/{fileName}";

                    // Set the path property on the grant
                    var property = typeof(Grant).GetProperty(pathProperty);
                    if (property != null)
                    {
                        property.SetValue(grant, relativePath);
                    }
                }
            }
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
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var grant = await _context.Grants
                .Include(g => g.BudgetItems)
                .FirstOrDefaultAsync(g => g.GrantId == id && g.UserId == currentUserId);

            if (grant == null)
            {
                return NotFound();
            }

            if (grant.Status != "In Progress")
            {
                TempData["ErrorMessage"] = "This grant application is locked and cannot be edited.";
                return RedirectToAction("Details", new { id = grant.GrantId });
            }

            grant.UserOptions = await GetUserOptionsAsync();

            return View(grant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Grant grant, string submitAction)
        {
            if (id != grant.GrantId)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            var existingGrant = await _context.Grants
                .FirstOrDefaultAsync(g => g.GrantId == id && g.UserId == currentUserId);

            if (existingGrant == null)
            {
                return NotFound();
            }

            if (existingGrant.Status != "In Progress")
            {
                TempData["ErrorMessage"] = "This grant application is locked and cannot be edited.";
                return RedirectToAction("Details", new { id = existingGrant.GrantId });
            }

            var isSubmitting = submitAction == "Submit";

            if (!isSubmitting)
            {
                ModelState.Remove(nameof(grant.Title));
                ModelState.Remove(nameof(grant.Description));
                ModelState.Remove(nameof(grant.Justification));
                ModelState.Remove(nameof(grant.AccountNumber));
                ModelState.Remove(nameof(grant.ProjectDirectorUserId));
                ModelState.Remove(nameof(grant.PrincipalInvestigatorUserId));

                grant.Title = string.IsNullOrWhiteSpace(grant.Title)
                    ? "Untitled Grant Application"
                    : grant.Title;

                grant.Description ??= string.Empty;
                grant.Justification ??= string.Empty;
                var currentUser = await _userManager.GetUserAsync(User);
                grant.AccountNumber = currentUser?.AccountNumber ?? 0;
            }

            if (isSubmitting &&
                grant.BenefitsMultipleDepartments &&
                grant.NumberOfDepartmentsBenefited == null)
            {
                ModelState.AddModelError(
                    nameof(grant.NumberOfDepartmentsBenefited),
                    "Enter how many departments will benefit.");
            }

            if (isSubmitting && grant.UsesHumanSubjects && grant.IRBApprovalFile == null)
            {
                ModelState.AddModelError(
                    nameof(grant.IRBApprovalFile),
                    "IRB Approval File is required when the project uses human subjects.");
            }

            var fileValidationErrors = ValidateUploadedFiles(grant);
            foreach (var error in fileValidationErrors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            if (!ModelState.IsValid)
            {
                grant.UserOptions = await GetUserOptionsAsync();
                return View(grant);
            }

            await ProcessFileUploads(grant);

            existingGrant.Title = grant.Title;
            existingGrant.Description = grant.Description;
            existingGrant.Justification = grant.Justification;
            existingGrant.AccountNumber = grant.AccountNumber;
            existingGrant.Timeline = grant.Timeline;
            existingGrant.ProjectDirectorUserId = grant.ProjectDirectorUserId;
            existingGrant.PrincipalInvestigatorUserId = grant.PrincipalInvestigatorUserId;
            existingGrant.WeberStateStudentsBenefited = grant.WeberStateStudentsBenefited;
            existingGrant.BenefitsMultipleDepartments = grant.BenefitsMultipleDepartments;
            existingGrant.NumberOfDepartmentsBenefited = grant.NumberOfDepartmentsBenefited;
            existingGrant.UsesHumanSubjects = grant.UsesHumanSubjects;
            existingGrant.HasMatchingFunds = grant.HasMatchingFunds;
            existingGrant.MatchingFundsAmount = grant.HasMatchingFunds ? grant.MatchingFundsAmount : null;
            existingGrant.AgreementOne = grant.AgreementOne;
            existingGrant.AgreementTwo = grant.AgreementTwo;
            existingGrant.AgreementThree = grant.AgreementThree;
            existingGrant.AgreementFour = grant.AgreementFour;
            existingGrant.ApplicationSignature = grant.ApplicationSignature;

            existingGrant.Status = isSubmitting ? "Submitted" : "In Progress";
            existingGrant.SubmittedAt = isSubmitting ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = isSubmitting
                ? "Grant request submitted successfully!"
                : "Grant application saved. You can come back and finish it later.";

            return RedirectToAction("Index", "UserDashboard");
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var grant = await _context.Grants
                .FirstOrDefaultAsync(g => g.GrantId == id && g.UserId == currentUserId);

            if (grant == null)
            {
                return NotFound();
            }

            return View(grant);
        }

        [Authorize(Roles = "ARCCmember,ARCCchair")]
        public async Task<IActionResult> Review(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            var grant = await _context.Grants
                .Include(g => g.BudgetItems)
                .FirstOrDefaultAsync(g =>
                    g.GrantId == id &&
                    (g.Status == "Submitted" ||
                     g.Status == "Approved by Department Chair" ||
                     g.Status == "Approved by Dean"));

            if (grant == null)
            {
                return NotFound();
            }

            var projectDirector = await _context.Users.FindAsync(grant.ProjectDirectorUserId);
            var principalInvestigator = await _context.Users.FindAsync(grant.PrincipalInvestigatorUserId);

            var viewModel = new GrantReviewDetailsViewModel
            {
                Grant = grant,
                ProjectDirectorName = projectDirector == null ? "Unknown user" : GetUserDisplayName(projectDirector),
                PrincipalInvestigatorName = principalInvestigator == null ? "Unknown user" : GetUserDisplayName(principalInvestigator),
                MoneyRequestedFromArcc = grant.BudgetItems.Sum(item => item.ARCCAmount),
                FileLinks = GetGrantFileLinks(grant),
                RubricScores = await GetRubricScoresAsync(grant.GrantId, currentUserId)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ARCCmember,ARCCchair")]
        public async Task<IActionResult> SaveReviewScores(int id, Dictionary<int, int> scores, string? arccChairApprovalNotes)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            var grantExists = await _context.Grants
                .AnyAsync(g =>
                g.GrantId == id &&
                (g.Status == "Submitted" ||
                 g.Status == "Approved by Department Chair" ||
                 g.Status == "Approved by Dean"));

            if (!grantExists)
            {
                return NotFound();
            }

            var criteria = await _context.RubricCriteria
                .Include(criterion => criterion.RatingSuggestions)
                .ToListAsync();

            foreach (var criterion in criteria)
            {
                if (!scores.TryGetValue(criterion.RubricCriterionId, out var selectedScore))
                {
                    TempData["ErrorMessage"] = "Choose a score for each rubric criterion before saving.";
                    return RedirectToAction(nameof(Review), new { id });
                }

                var allowedScores = criterion.RatingSuggestions.Select(suggestion => suggestion.Score).ToHashSet();
                if (!allowedScores.Contains(selectedScore))
                {
                    TempData["ErrorMessage"] = "One or more selected scores are not valid for the current rubric.";
                    return RedirectToAction(nameof(Review), new { id });
                }
            }

            var existingScores = await _context.GrantRubricScores
                .Where(score => score.GrantId == id && score.ReviewerUserId == currentUserId)
                .ToListAsync();

            foreach (var criterion in criteria)
            {
                var selectedScore = scores[criterion.RubricCriterionId];
                var existingScore = existingScores
                    .FirstOrDefault(score => score.RubricCriterionId == criterion.RubricCriterionId);

                if (existingScore == null)
                {
                    _context.GrantRubricScores.Add(new GrantRubricScore
                    {
                        GrantId = id,
                        RubricCriterionId = criterion.RubricCriterionId,
                        ReviewerUserId = currentUserId,
                        Score = selectedScore
                    });
                }
                else
                {
                    existingScore.Score = selectedScore;
                }
            }

            var grant = await _context.Grants.FindAsync(id);
            if (grant == null)
            {
                return NotFound();
            }

            grant.ArccChairApprovalNotes = arccChairApprovalNotes?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rubric scores saved.";
            return RedirectToAction(nameof(Review), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> BudgetWorksheet(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var grant = await _context.Grants
                .Include(g => g.BudgetItems)
                .FirstOrDefaultAsync(g => g.GrantId == id && g.UserId == currentUserId);

            if (grant == null) return NotFound();

            var vm = new BudgetWorksheetViewModel
            {
                GrantId = grant.GrantId,
                GrantTitle = grant.Title,
                BudgetItems = grant.BudgetItems.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BudgetWorksheet(int id, BudgetWorksheetViewModel vm)
        {
            var currentUserId = _userManager.GetUserId(User);
            var grant = await _context.Grants
                .FirstOrDefaultAsync(g => g.GrantId == id && g.UserId == currentUserId);

            if (grant == null) return NotFound();

            // Replace all budget items for this grant
            var oldItems = _context.BudgetItems.Where(b => b.GrantId == id);
            _context.BudgetItems.RemoveRange(oldItems);

            foreach (var item in vm.BudgetItems)
            {
                item.GrantId = id;
                item.BudgetItemId = 0;
                _context.BudgetItems.Add(item);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Budget worksheet saved!";
            return RedirectToAction("BudgetWorksheet", new { id });
        }

        private static string GetUserDisplayName(ApplicationUser user)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            return string.IsNullOrWhiteSpace(fullName)
                ? user.Email ?? user.UserName ?? user.Id
                : fullName;
        }

        private static List<GrantFileLinkViewModel> GetGrantFileLinks(Grant grant)
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

        private async Task<List<GrantReviewCriterionScoreViewModel>> GetRubricScoresAsync(int grantId, string reviewerUserId)
        {
            var criteria = await _context.RubricCriteria
                .Include(criterion => criterion.RatingSuggestions)
                .OrderBy(criterion => criterion.RubricCriterionId)
                .ToListAsync();

            var savedScores = await _context.GrantRubricScores
                .Where(score => score.GrantId == grantId && score.ReviewerUserId == reviewerUserId)
                .ToDictionaryAsync(score => score.RubricCriterionId, score => score.Score);

            return criteria
                .Select(criterion => new GrantReviewCriterionScoreViewModel
                {
                    RubricCriterionId = criterion.RubricCriterionId,
                    Name = criterion.Name,
                    Description = criterion.Description,
                    MaximumScore = criterion.MaximumScore,
                    SelectedScore = savedScores.TryGetValue(criterion.RubricCriterionId, out var savedScore)
                        ? savedScore
                        : null,
                    RatingSuggestions = criterion.RatingSuggestions
                        .OrderByDescending(suggestion => suggestion.Score)
                        .ToList()
                })
                .ToList();
        }
    }
}
