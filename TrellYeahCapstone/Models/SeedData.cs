using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;

namespace TrellYeahCapstone.Models
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);

            var seedUsers = await SeedRegularUsersAsync(userManager);

            await SeedCollegesAsync(db, seedUsers);
            await SeedDepartmentsAsync(db, seedUsers);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
        {
            var adminEmail = "admin@4760weber.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Password1!");
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        private static async Task<List<IdentityUser>> SeedRegularUsersAsync(UserManager<IdentityUser> userManager)
        {
            var seedUsers = new List<IdentityUser>();

            for (var i = 1; i <= 6; i++)
            {
                var userEmail = $"userAccount{i}@gmail.com";
                var seedUser = await userManager.FindByEmailAsync(userEmail);

                if (seedUser == null)
                {
                    seedUser = new IdentityUser
                    {
                        UserName = userEmail,
                        Email = userEmail,
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(seedUser, "Password1!");
                }

                seedUsers.Add(seedUser);
            }

            return seedUsers;
        }

        private static async Task SeedCollegesAsync(ApplicationDbContext db, List<IdentityUser> seedUsers)
        {
            var collegeSeedData = new[]
            {
                new { Name = "engeneering", Dean = seedUsers[0] },
                new { Name = "applied science", Dean = seedUsers[1] },
                new { Name = "technology", Dean = seedUsers[2] }
            };

            foreach (var collegeSeed in collegeSeedData)
            {
                var college = await db.Colleges.FirstOrDefaultAsync(c => c.Name == collegeSeed.Name);

                if (college == null)
                {
                    db.Colleges.Add(new College
                    {
                        Name = collegeSeed.Name,
                        DeanUserId = collegeSeed.Dean.Id
                    });
                }
                else
                {
                    college.DeanUserId = collegeSeed.Dean.Id;
                }
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedDepartmentsAsync(ApplicationDbContext db, List<IdentityUser> seedUsers)
        {
            var engineeringCollege = await db.Colleges.FirstAsync(c => c.Name == "engeneering");
            var appliedScienceCollege = await db.Colleges.FirstAsync(c => c.Name == "applied science");
            var technologyCollege = await db.Colleges.FirstAsync(c => c.Name == "technology");

            var departmentSeedData = new[]
            {
                new { Name = "School of Computing", College = technologyCollege, Chair = seedUsers[3] },
                new { Name = "School of Engineering", College = engineeringCollege, Chair = seedUsers[4] },
                new { Name = "School of Biology", College = appliedScienceCollege, Chair = seedUsers[5] }
            };

            foreach (var departmentSeed in departmentSeedData)
            {
                var department = await db.Departments.FirstOrDefaultAsync(d => d.Name == departmentSeed.Name);

                if (department == null)
                {
                    db.Departments.Add(new Department
                    {
                        Name = departmentSeed.Name,
                        CollegeId = departmentSeed.College.Id,
                        ChairUserId = departmentSeed.Chair.Id
                    });
                }
                else
                {
                    department.CollegeId = departmentSeed.College.Id;
                    department.ChairUserId = departmentSeed.Chair.Id;
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
