using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using LocationDeVoiture.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            
            // Configure Entity Framework with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Configure cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Register background service for automatic payments
            builder.Services.AddHostedService<PaymentBackgroundService>();

            var app = builder.Build();

            // Ensure database is created and seeded
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<ApplicationDbContext>();
                
                // Recreate database with new schema
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                
                // Seed admin user
                await SeedAdminUser(services, app.Configuration);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }

        private static async Task SeedAdminUser(IServiceProvider services, IConfiguration configuration)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user. The password is never hardcoded: it comes from
            // configuration (Seed:AdminPassword in appsettings*.json or user secrets)
            // or from the SEED_ADMIN_PASSWORD environment variable, which wins if set.
            var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@autorent.ma";
            var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                adminPassword = configuration["Seed:AdminPassword"];
            }
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "No admin password configured. Set the configuration key 'Seed:AdminPassword' " +
                    "(for example with 'dotnet user-secrets set \"Seed:AdminPassword\" \"<password>\"' " +
                    "or the environment variable Seed__AdminPassword) or set SEED_ADMIN_PASSWORD.");
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nom = "Admin",
                    Prenom = "System",
                    Ville = "Casablanca",
                    EmailConfirmed = true,
                    DateInscription = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Could not create the seeded admin user: {errors}");
                }
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
