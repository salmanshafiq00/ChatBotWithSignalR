global using ChatBotWithSignalR.Entity;
using ChatBotWithSignalR.Constant;
using ChatBotWithSignalR.Data;
using ChatBotWithSignalR.Interface;
using ChatBotWithSignalR.Permission;
using ChatBotWithSignalR.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

//builder.Services.AddAuthorization(options =>
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

builder.Services.AddTransient<IToastNotification, ToastNotification>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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
        pattern: "{area:exists}/{controller}/{action}"
    );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
