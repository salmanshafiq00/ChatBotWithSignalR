﻿using ChatBotWithSignalR.Constant;
using Microsoft.AspNetCore.Identity;

namespace ChatBotWithSignalR.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync (UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
        }
    }
}
