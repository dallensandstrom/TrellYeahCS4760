using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrellYeahCapstone.Models
{
    public class ArccGrantSearchViewModel
    {
        public string? SearchTerm { get; set; }

        public string? Status { get; set; }

        public int? CollegeId { get; set; }

        public int? DepartmentId { get; set; }

        public decimal? MinAverageScore { get; set; }

        public decimal? MaxAverageScore { get; set; }

        public string SortBy { get; set; } = "submitted_desc";

        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<SelectListItem> CollegeOptions { get; set; } = new();

        public List<SelectListItem> DepartmentOptions { get; set; } = new();

        public List<SelectListItem> SortOptions { get; set; } = new();

        public List<ArccGrantSearchResultViewModel> Results { get; set; } = new();
    }
}
