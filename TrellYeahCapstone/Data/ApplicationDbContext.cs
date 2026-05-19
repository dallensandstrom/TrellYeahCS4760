using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Models;

namespace TrellYeahCapstone.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<College> Colleges { get; set; }

        public DbSet<Department> Departments { get; set; }
    }
}
