using ChatBotWithSignalR.Areas.Admin.Models;
using ChatBotWithSignalR.Constant;
using ChatBotWithSignalR.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChatBotWithSignalR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IToastNotification _toast;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IToastNotification toast)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _toast = toast;
        }
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> LoadUser(string roleId = "")
        {
            try
            {
                IList<ApplicationUser> users = new List<ApplicationUser>();
                List<ApplicationUserViewModel> userList = new();
                if (string.IsNullOrEmpty(roleId))
                    users = await _userManager.Users.ToListAsync();
                else
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    users = await _userManager.GetUsersInRoleAsync(role?.Name ?? "");
                }
                foreach (var user in users)
                {
                    userList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePhotoUrl = user.ProfilePhotoUrl,
                        IsChatUser = user.IsChatUser,
                        Roles = string.Join(",", await _userManager.GetRolesAsync(user))
                    });
                }
                return PartialView("_ViewAll", userList);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnCreateOrEdit(string userId = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return View("CreateOrEdit");
                }
                else
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        await _toast.ToastError("User Not Found");
                        return RedirectToAction(nameof(Index));
                    }
                    ApplicationUserViewModel userViewModel = new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePhotoUrl = user.ProfilePhotoUrl,
                        IsChatUser = user.IsChatUser,
                    };
                    return View("CreateOrEdit", userViewModel);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> OnPostCreateOrEdit(ApplicationUserViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    ApplicationUser user = new();
                    user.FirstName = model.FirstName.Trim();
                    user.LastName = model.LastName.Trim();
                    user.UserName = model.UserName.Trim();
                    user.Email = model.Email.Trim();
                    user.PhoneNumber = model.PhoneNumber.Trim();
                    user.IsChatUser = model.IsChatUser;
                    user.EmailConfirmed = true;
                    await _userManager.AddPasswordAsync(user, model.Password);
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await _toast.ToastSuccess("User Create Successfully");
                        return RedirectToAction(nameof(Index));
                    }
                    await _toast.ToastError($"{result.Errors}");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var user = await _userManager.FindByIdAsync(model.Id);
                    if (user == null)
                    {
                        await _toast.ToastError("User Not Found");
                        return RedirectToAction(nameof(Index));
                    }
                    user.FirstName = model.FirstName.Trim(); 
                    user.LastName = model.LastName.Trim();
                    user.Email = model?.Email.Trim();
                    user.PhoneNumber = model?.PhoneNumber.Trim();
                    user.IsChatUser = model.IsChatUser;
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        await _toast.ToastSuccess($"{user.UserName} Update Successfully");
                        return RedirectToAction(nameof(Index));
                    }
                    await _toast.ToastError($"{result.Errors}");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostUserDelete(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    await _toast.ToastError("User Not Found");
                    return RedirectToAction(nameof(Index));
                }
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await _toast.ToastSuccess($"{user.UserName} Deleted Successfully");
                    return RedirectToAction(nameof(Index));
                }
                await _toast.ToastError($"{result.Errors}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region User Role Manage

        [HttpGet]
        public async Task<IActionResult> OnGetUserRoles(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    await _toast.ToastError("User Not Found");
                    return RedirectToAction(nameof(Index));
                }
                ApplicationUserViewModel userModel = new()
                {
                    Id = user.Id,
                    UserName = user.UserName
                };
                var roles = await _roleManager.Roles.ToListAsync();
                foreach (var role in roles)
                {
                    userModel.UserRoles.Add(new UserRolesViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                    });
                }
                return View("UserRoles", userModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostUserRoles(ApplicationUserViewModel model, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    await _toast.ToastError("User Not Found");
                    return RedirectToAction(nameof(Index));
                }
                var userRoles = await _userManager.GetRolesAsync(user);
                var result = await _userManager.RemoveFromRolesAsync(user, userRoles);
                if (!result.Succeeded)
                {
                    await _toast.ToastError($"{result.Errors}");
                    return RedirectToAction(nameof(Index));
                }
                result = await _userManager
                    .AddToRolesAsync(user, model.UserRoles
                        .Where(r => r.IsSelected).Select(r => r.RoleName)
                        .ToList());
                if (result.Succeeded)
                {
                    await _toast.ToastSuccess("User Roles Update Successfully");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    await _toast.ToastError($"{result.Errors}");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion


        public async Task<IActionResult> IsEmailUsed(string email, string id)
        {
            var userById = await _userManager.FindByIdAsync(id);
            var userByEmail = await _userManager.FindByEmailAsync(email);
            if (userById is null && userByEmail is null)
                return Json(true);
            else if (userById.Email == userByEmail.Email)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already used");
            }
        }
    }
}
