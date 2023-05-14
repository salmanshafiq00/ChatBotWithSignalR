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

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddSignInManager<CustomSignInManager<ApplicationUser>>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();


// When manully create policy for every permission.
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
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
builder.Services.AddTransient<IToastNotification, ToastNotification>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddSignalR();

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
        pattern: "{area:exists}/{controller}/{action}",
        defaults: new {area = "Chat", controller = "Chat", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{area:exists}/{controller=Chat}/{action=Index}");
app.MapRazorPages();
app.MapHub<ChatHub>("/Chat");

app.Run();
