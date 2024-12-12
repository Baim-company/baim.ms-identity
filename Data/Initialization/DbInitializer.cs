using Identity.API.Data.DbContexts;
using Identity.API.Data.Dtos.ExternalUsers;
using Identity.API.Data.Entities;
using Identity.API.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace Identity.API.Data.Initialization;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await context.Database.MigrateAsync();


        await InitializeRolesAsync(serviceProvider);

        await InitializeAdminAsync(serviceProvider);
    }



    private static async Task InitializeRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var roles = new Dictionary<string, string>
        {
            { "Admin", "Administrator role with full access" },
            { "Staff", "Staff role with limited access" },
            { "UserAdmin", "User administration role" },
            { "User", "Regular user role" }
        };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Key))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role.Key,
                    Description = role.Value
                });
            }
        }
    }




    private static async Task InitializeAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var externalUserSyncService = scope.ServiceProvider.GetRequiredService<IExternalUserSyncService>();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();


        var adminEmail = configuration["AdminSettings:Email"];
        var adminPassword = configuration["AdminSettings:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            throw new Exception("Admin credentials are not configured properly in the configuration.");


        var adminUser = await CreateAdminUserIfNotExistsAsync(userManager, adminEmail, adminPassword);

        if (adminUser != null)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");

            var accessToken = await tokenService.GenerateTokenAsync(adminUser);


            var externalUserDto = MapToExternalUserDto(adminUser);
            var addAdminResponse = await externalUserSyncService.AddAdminAsync(externalUserDto, accessToken);

            if (addAdminResponse != null)
                Console.WriteLine("Admin created successfully in external system.");
            else
                Console.WriteLine("Failed to create admin in external system.");
        }
    }



    private static async Task<ApplicationUser?> CreateAdminUserIfNotExistsAsync(UserManager<ApplicationUser> userManager, string adminEmail, string adminPassword)
    {
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null) return null;

        var adminUser = new ApplicationUser
        {
            Name = "Admin",
            Email = adminEmail,
            UserName = adminEmail,
            NormalizedEmail = adminEmail.ToUpper(),
            EmailConfirmed = true,
            AvatarName = "admin-icon.png",
            AvatarPath = "default/admin-icon.png",
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded) return adminUser;

        throw new Exception($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }



    private static ExternalUserDto MapToExternalUserDto(ApplicationUser adminUser)
    {
        return new ExternalUserDto
        {
            Id = adminUser.Id,
            Email = adminUser.Email!,
            Name = adminUser.Name,
            Role = "Admin",
            AvatarPath = adminUser.AvatarPath,
        };
    }
}
