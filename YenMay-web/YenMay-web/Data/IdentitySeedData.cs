using Microsoft.AspNetCore.Identity;
using YenMay_web.Models.Domain;

namespace YenMay_web.Data
{
    public static class IdentitySeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole<int>>>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            // ============================
            // 1. SEED ROLES
            // ============================
            string[] roles = { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(
                        new IdentityRole<int>(role)
                    );

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            $"Tạo role {role} thất bại: " +
                            string.Join(", ", roleResult.Errors.Select(e => e.Description))
                        );
                    }
                }
            }

            // ============================
            // 2. SEED ADMIN USER
            // ============================
            string adminUserName = "baochauisdabet@gmail.com";
            string adminEmail = "baochauisdabet@gmail.com";
            string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Address = "Hồ Chí Minh",
                    PhoneNumber = "0000000000"
                };

                var createUserResult = await userManager.CreateAsync(
                    adminUser,
                    adminPassword
                );

                if (!createUserResult.Succeeded)
                {
                    throw new Exception(
                        "Không thể tạo admin user: " +
                        string.Join(", ", createUserResult.Errors.Select(e => e.Description))
                    );
                }
            }

            // ============================
            // 3. GÁN ROLE ADMIN
            // ============================
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                if (!addRoleResult.Succeeded)
                {
                    throw new Exception(
                        "Không thể gán role Admin: " +
                        string.Join(", ", addRoleResult.Errors.Select(e => e.Description))
                    );
                }
            }
        }
    }
}
