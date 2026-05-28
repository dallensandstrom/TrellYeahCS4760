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
        public async Task<IActionResult> Create(Grant grant, string submitAction)
        {
            var isSubmitting = submitAction == "Submit";

            if (!isSubmitting)
            {
                ModelState.Remove(nameof(grant.Title));
                ModelState.Remove(nameof(grant.Description));
                ModelState.Remove(nameof(grant.Justification));
                ModelState.Remove(nameof(grant.ProjectDirectorUserId));
                ModelState.Remove(nameof(grant.PrincipalInvestigatorUserId));

                var currentUserId = _userManager.GetUserId(User) ?? string.Empty;

                grant.Title = string.IsNullOrWhiteSpace(grant.Title)
                    ? "Untitled Grant Application"
                    : grant.Title;

                grant.Description ??= string.Empty;
                grant.Justification ??= string.Empty;

                grant.ProjectDirectorUserId = string.IsNullOrWhiteSpace(grant.ProjectDirectorUserId)
                    ? currentUserId
                    : grant.ProjectDirectorUserId;

                grant.PrincipalInvestigatorUserId = string.IsNullOrWhiteSpace(grant.PrincipalInvestigatorUserId)
                    ? currentUserId
                    : grant.PrincipalInvestigatorUserId;
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
                .FirstOrDefaultAsync(g => g.GrantId == id && g.UserId == currentUserId);

            if (grant == null)
            {
                return NotFound();
            }

            if (grant.Status == "Submitted")
            {
                TempData["ErrorMessage"] = "Submitted grant applications are locked and cannot be edited.";
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

            if (existingGrant.Status == "Submitted")
            {
                TempData["ErrorMessage"] = "Submitted grant applications are locked and cannot be edited.";
                return RedirectToAction("Details", new { id = existingGrant.GrantId });
            }

            var isSubmitting = submitAction == "Submit";

            if (!isSubmitting)
            {
                ModelState.Remove(nameof(grant.Title));
                ModelState.Remove(nameof(grant.Description));
                ModelState.Remove(nameof(grant.Justification));
                ModelState.Remove(nameof(grant.ProjectDirectorUserId));
                ModelState.Remove(nameof(grant.PrincipalInvestigatorUserId));

                grant.Title = string.IsNullOrWhiteSpace(grant.Title)
                    ? "Untitled Grant Application"
                    : grant.Title;

                grant.Description ??= string.Empty;
                grant.Justification ??= string.Empty;
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
            existingGrant.Timeline = grant.Timeline;
            existingGrant.ProjectDirectorUserId = grant.ProjectDirectorUserId;
            existingGrant.PrincipalInvestigatorUserId = grant.PrincipalInvestigatorUserId;
            existingGrant.WeberStateStudentsBenefited = grant.WeberStateStudentsBenefited;
            existingGrant.BenefitsMultipleDepartments = grant.BenefitsMultipleDepartments;
            existingGrant.NumberOfDepartmentsBenefited = grant.NumberOfDepartmentsBenefited;
            existingGrant.UsesHumanSubjects = grant.UsesHumanSubjects;

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
    }
}