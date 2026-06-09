using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrellYeahCapstone.Data;
using TrellYeahCS4760.Models;

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
            await SeedDepartmentsAsync(userManager, db, seedUsers);
            await SeedArccChairAsync(userManager, seedUsers);
            await SeedArccMemberAsync(userManager, seedUsers);
            await SeedRubricAsync(db);
            await SeedSubmittedGrantAsync(db, seedUsers);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "ARCCchair", "ARCCmember", "DeptChair" };

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

        private static async Task SeedDepartmentsAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext db, List<ApplicationUser> seedUsers)
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
                    await userManager.UpdateAsync(departmentSeed.Chair);
                    await userManager.AddToRoleAsync(departmentSeed.Chair, "DeptChair");
                }
                else
                {
                    department.CollegeId = departmentSeed.College.Id;
                    department.ChairUserId = departmentSeed.Chair.Id;
                    await userManager.UpdateAsync(departmentSeed.Chair);
                    await userManager.AddToRoleAsync(departmentSeed.Chair, "DeptChair");
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
            if (!await userManager.IsInRoleAsync(arccChair, "ARCCmember"))
            {
                await userManager.AddToRoleAsync(arccChair, "ARCCmember");
            }
        }

        private static async Task SeedArccMemberAsync(UserManager<ApplicationUser> userManager, List<ApplicationUser> seedUsers)
        {
            var arccMember = seedUsers[2];
            arccMember.IsArccCommitteeMember = true;
            arccMember.IsArccCommitteeChair = false;

            await userManager.UpdateAsync(arccMember);

            if (!await userManager.IsInRoleAsync(arccMember, "ARCCmember"))
            {
                await userManager.AddToRoleAsync(arccMember, "ARCCmember");
            }

            if (await userManager.IsInRoleAsync(arccMember, "ARCCchair"))
            {
                await userManager.RemoveFromRoleAsync(arccMember, "ARCCchair");
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

        private static async Task SeedSubmittedGrantAsync(ApplicationDbContext db, List<ApplicationUser> seedUsers)
        {
            var grantOwner = seedUsers[0];
            var grant = await db.Grants
                .Include(g => g.BudgetItems)
                .FirstOrDefaultAsync(g => g.Title == "TestGrant1");

            if (grant == null)
            {
                grant = new Grant
                {
                    Title = "TestGrant1",
                    UserId = grantOwner.Id,
                    SubmittedAt = new DateTime(2026, 6, 9, 12, 0, 0),
                    Status = "Submitted"
                };

                db.Grants.Add(grant);
            }

            grant.Description = "This is a test grant for seed data.";
            grant.Justification = "This is to test our program.";
            grant.Timeline = "Today";
            grant.ProjectDirectorUserId = grantOwner.Id;
            grant.PrincipalInvestigatorUserId = grantOwner.Id;
            grant.WeberStateStudentsBenefited = 4;
            grant.BenefitsMultipleDepartments = true;
            grant.NumberOfDepartmentsBenefited = 2;
            grant.UsesHumanSubjects = false;
            grant.SupportingDocument1Path = "/seed-files/TestFile1.pdf";
            grant.SupportingDocument2Path = null;
            grant.SupportingDocument3Path = null;
            grant.IRBApprovalFilePath = null;
            grant.Status = "Submitted";
            grant.SubmittedAt ??= new DateTime(2026, 6, 9, 12, 0, 0);

            await db.SaveChangesAsync();

            var budgetItem = grant.BudgetItems.FirstOrDefault(item => item.ItemName == "Stuff");

            if (budgetItem == null)
            {
                budgetItem = new BudgetItem
                {
                    GrantId = grant.GrantId,
                    ItemName = "Stuff"
                };

                db.BudgetItems.Add(budgetItem);
            }

            budgetItem.Quantity = 1;
            budgetItem.ItemType = "Hardware";
            budgetItem.ARCCAmount = 100;
            budgetItem.CollegeAmount = 200;
            budgetItem.DepartmentAmount = 300;
            budgetItem.OtherAmount = 400;
            budgetItem.OtherSource = null;

            await db.SaveChangesAsync();
        }
    }
}
