using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

// Dallen Sandstrom - Used for user authentication and authorization.
namespace TrellYeahCapstone.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public int? CollegeId { get; set; }
        public int? DepartmentId { get; set; }

        public bool IsArccCommitteeMember { get; set; }

        public bool IsArccCommitteeChair { get; set; }
    }
}
