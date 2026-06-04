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
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);

            var seedUsers = await SeedRegularUsersAsync(userManager);

            await SeedCollegesAsync(db, seedUsers);
            await SeedDepartmentsAsync(db, seedUsers);
            await SeedArccChairAsync(userManager, seedUsers);
            await SeedRubricAsync(db);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "ARCCchair" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@4760weber.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Password1!");
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        private static async Task<List<ApplicationUser>> SeedRegularUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var seedUsers = new List<ApplicationUser>();

            for (var i = 1; i <= 6; i++)
            {
                var userEmail = $"userAccount{i}@gmail.com";
                var seedUser = await userManager.FindByEmailAsync(userEmail);

                if (seedUser == null)
                {
                    seedUser = new ApplicationUser
                    {
                        UserName = userEmail,
                        Email = userEmail,
                        FirstName = "Regular",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(seedUser, "Password1!");
                }

                seedUsers.Add(seedUser);
            }

            return seedUsers;
        }

        private static async Task SeedCollegesAsync(ApplicationDbContext db, List<ApplicationUser> seedUsers)
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

        private static async Task SeedDepartmentsAsync(ApplicationDbContext db, List<ApplicationUser> seedUsers)
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

        private static async Task SeedArccChairAsync(UserManager<ApplicationUser> userManager, List<ApplicationUser> seedUsers)
        {
            var arccChair = seedUsers[1];
            var currentChairs = await userManager.Users
                .Where(user => user.IsArccCommitteeChair)
                .ToListAsync();

            foreach (var chair in currentChairs.Where(user => user.Id != arccChair.Id))
            {
                chair.IsArccCommitteeChair = false;
                await userManager.UpdateAsync(chair);
                await userManager.RemoveFromRoleAsync(chair, "ARCCchair");
            }

            arccChair.IsArccCommitteeMember = true;
            arccChair.IsArccCommitteeChair = true;
            await userManager.UpdateAsync(arccChair);

            if (!await userManager.IsInRoleAsync(arccChair, "ARCCchair"))
            {
                await userManager.AddToRoleAsync(arccChair, "ARCCchair");
            }
        }

        private static async Task SeedRubricAsync(ApplicationDbContext db)
        {
            var criterion = await db.RubricCriteria
                .Include(c => c.RatingSuggestions)
                .FirstOrDefaultAsync(c => c.Name == "Educational Impact");

            if (criterion == null)
            {
                criterion = new RubricCriterion
                {
                    Name = "Educational Impact",
                    Description = "Evaluate how the proposed project enhances the educational experience at Weber State University.",
                    MaximumScore = 30
                };

                db.RubricCriteria.Add(criterion);
                await db.SaveChangesAsync();
            }
            else
            {
                criterion.Description = "Evaluate how the proposed project enhances the educational experience at Weber State University.";
                criterion.MaximumScore = 30;
            }

            var suggestionSeedData = new[]
            {
                new { Score = 30, Description = "Strongly enhances student learning; impacts many students across multiple departments/programs." },
                new { Score = 20, Description = "Meaningfully improves student learning; impacts a moderate number of students or one major program." },
                new { Score = 10, Description = "Provides limited educational benefit; impacts a small group of students." },
                new { Score = 0, Description = "Little or no clear educational impact." }
            };

            foreach (var suggestionSeed in suggestionSeedData)
            {
                var suggestion = criterion.RatingSuggestions
                    .FirstOrDefault(s => s.Score == suggestionSeed.Score);

                if (suggestion == null)
                {
                    db.RubricRatingSuggestions.Add(new RubricRatingSuggestion
                    {
                        RubricCriterionId = criterion.RubricCriterionId,
                        Score = suggestionSeed.Score,
                        Description = suggestionSeed.Description
                    });
                }
                else
                {
                    suggestion.Description = suggestionSeed.Description;
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
