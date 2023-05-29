global using ChatBotWithSignalR.Entity;
using ChatBotWithSignalR.Data;
using ChatBotWithSignalR.DTOs;
using ChatBotWithSignalR.Hubs;
using ChatBotWithSignalR.Interface;
using ChatBotWithSignalR.Permission;
using ChatBotWithSignalR.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
services.AddDatabaseDeveloperPageExceptionFilter();


services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    })
    .AddTwitter(options =>
    {
        options.ConsumerKey = configuration["Authentication:Twitter:ClientId"];
        options.ConsumerSecret = configuration["Authentication:Twitter:ClientSecret"];
        options.RetrieveUserDetails = true;
    }).AddFacebook(options =>
    {
        options.ClientId = configuration["Authentication:Facebook:ClientId"];
        options.ClientSecret = configuration["Authentication:Facebook:ClientSecret"];
    });

services.AddControllersWithViews();

services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddSignInManager<CustomSignInManager<ApplicationUser>>();



services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();


// When manully create policy for every permission.
//services.AddAuthorization(options =>
//{
//    options.AddPolicy(Permissions.ApplicationUsers.Delete, builder =>
//    {
//        builder.AddRequirements(new PermissionRequirement(Permissions.ApplicationUsers.Delete));
//    });
//    options.AddPolicy(Permissions.IdentityRoles.Delete, builder =>
//    {
//        builder.AddRequirements(new PermissionRequirement(Permissions.IdentityRoles.Delete));
//    });
//    // These goes on for every permission
//});
services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
services.AddTransient<IToastNotification, ToastNotification>();
services.AddTransient<IMailService, MailService>();
services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();


    // Here seeding data of Superadmin user, role 
    using (var scope = app.Services.CreateScope())
    {
        var seedingSevice = scope.ServiceProvider;
        var userManager = seedingSevice.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = seedingSevice.GetRequiredService<RoleManager<IdentityRole>>();
        await ChatBotWithSignalR.Seeds.DefaultRoles.SeedAsync(userManager, roleManager);
        await ChatBotWithSignalR.Seeds.DefaultUsers.SeedSuperAdminAsync(userManager, roleManager);
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
        name: "areaRoute",
        pattern: "{area:exists}/{controller=Chat}/{action=Index}/{id?}",
        defaults: new { area = "Chat", controller = "Chat", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapRazorPages();
//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{area=exists}/{controller}/{action}/{id?}",
//        defaults: new { area = "Chat", controller = "Chat", action = "Index" });
//});

app.MapHub<ChatHub>("/Chat");

app.Run();
