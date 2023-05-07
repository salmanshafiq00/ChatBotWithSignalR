using ChatBotWithSignalR.Areas.Chat.Models;
using ChatBotWithSignalR.Data;
using ChatBotWithSignalR.Hubs;
using ChatBotWithSignalR.Interface;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Imaging;
using System.Drawing;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ChatBotWithSignalR.Areas.Chat.Controllers
{
    [Area("Chat")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IToastNotification _toast;
        private readonly IWebHostEnvironment _webHost;

        public ChatController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHubContext<ChatHub> chatHubContext, IToastNotification toast, IWebHostEnvironment webHost)
        {
            _userManager = userManager;
            _context = context;
            _chatHubContext = chatHubContext;
            _toast = toast;
            _webHost = webHost;
        }
        public async Task<IActionResult> Index()
        {
            ChatUserGroupViewModel model = new();
            var loginUser = await _userManager.GetUserAsync(User);
            model.LoginUserId = loginUser.Id;
            model.LoginUserFullName = string.Concat(loginUser.FirstName, " ", loginUser.LastName) ?? loginUser.UserName;

            var userList = await _userManager.Users.Where(p => p.IsChatUser).ToListAsync();
            foreach (var user in userList)
            {
                model.ChatUsers.Add(new ChatUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = string.Concat(user.FirstName, " ", user.LastName),
                    UserName = user.UserName,
                    Email = user.Email,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    UnreadMessageNo = await GetUnreadMessageNo(user.Id)
                });
            }

            var userInGroups = await _context.UserGroups.Where(u => u.UserId == model.LoginUserId).ToListAsync();
            foreach (var user in userInGroups)
            {
                var group = await _context.ChatGroups.FirstOrDefaultAsync(g => g.Id == user.GroupId && !g.IsDeleted);
                if (group is not null)
                {
                    model.ChatGroupsList.Add(group);
                }
            }
            //model.ChatUsers.RemoveAt(model.ChatUsers.FindIndex(p => p.Id == model.LoginUserId));
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LoadConversationByUserId(string toUserId)
        {
            try
            {
                ConversationViewModel conversationViewModel = new();
                var toUser = await _userManager.FindByIdAsync(toUserId);
                var loginUser = await _userManager.GetUserAsync(User);
                if (loginUser is null)
                {
                    await _toast.ToastError("User not found");
                    return new JsonResult(false);
                }
                conversationViewModel.ToUserId = toUserId;
                conversationViewModel.ToUserName = string.IsNullOrEmpty(string.Concat(toUser.FirstName, " ", toUser.LastName)) ? toUser.UserName : string.Concat(toUser.FirstName, " ", toUser.LastName);
                conversationViewModel.ToUserProfilePhotoUrl = toUser.ProfilePhotoUrl;
                conversationViewModel.LoginUserId = loginUser.Id;

                var conversations = await _context.Conversations.Where(c => (c.FromUserId == loginUser.Id || c.FromUserId == toUserId) && (c.ToUserId == loginUser.Id || c.ToUserId == toUserId) && c.GroupId == 0).ToListAsync();
                conversationViewModel.Conversations = conversations;
                return PartialView("_Conversations", conversationViewModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation(Conversation conversation)
        {
            using var transection = await _context.Database.BeginTransactionAsync();
            try
            {
                if (ModelState.IsValid && conversation is not null)
                {
                    if (conversation?.Files?.Count > 0)
                    {
                        foreach (IFormFile item in conversation.Files)
                        {
                            var fileSize = string.Empty;
                            var fileUrl = string.Empty;
                            (fileUrl, fileSize) = await SaveImageAsync(item, conversation.FromUserId);
                            conversation.ConversationFiles.Add(new ConversationFile
                            {
                                FileName = item.FileName,
                                FileSize = fileSize,
                                FileType = item.ContentType,
                                FileUrl = fileUrl
                            });
                        }
                    }
                    var entity = await _context.Conversations.AddAsync(conversation);
                    var affectedRow = await _context.SaveChangesAsync();
                    await transection.CommitAsync(new CancellationToken());

                    // Send conversation to user/group using signalr
                    conversation.ToShortTime = conversation.SendDate.ToShortTimeString();
                    if (affectedRow > 0)
                    {
                        // Send conversation to user using signalr
                        if (conversation.GroupId == 0)
                        {
                            await _chatHubContext.Clients.User(conversation.ToUserId).SendAsync("ReceiveMessages", conversation);
                        }
                        else  // Send conversation to group using signalr
                        {
                            var result = await _context.UserGroups.Where(p => p.GroupId == conversation.GroupId).ToListAsync();
                            result.RemoveAt(result.FindIndex(p => p.UserId == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)));
                            foreach (var group in result)
                            {
                                await _chatHubContext.Clients.User(group.UserId).SendAsync("ReceiveGroupMessages", conversation);
                            }
                        }
                    }
                    return new JsonResult(new { IsSuccess = true, Msg = entity.Entity.TextMessage, Time = entity.Entity.SendDate, ToUserId = conversation.ToUserId, GroupId = conversation.GroupId });
                }
                else
                {
                    foreach (var error in ModelState.Values.SelectMany(e => e.Errors))
                    {
                        await _toast.ToastError(error.ErrorMessage);
                        break;
                    }
                    return new JsonResult(new { IsSuccess = false });

                }
            }
            catch
            {
                await transection.RollbackAsync(new CancellationToken());
                foreach (var file in conversation.ConversationFiles)
                {
                    if (!String.IsNullOrEmpty(file.FileUrl))
                    {
                        var paths = _webHost.WebRootPath + $"{(file.FileUrl).Replace('/', '\\')}";
                        if (System.IO.File.Exists(paths))
                        {
                            System.IO.File.Delete(paths);
                        }
                    }
                }
                return new JsonResult(new { IsSuccess = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVisibilityStatus(string fromUserId)
        {
            try
            {
                var loginUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _context.Conversations.Where(c => c.FromUserId == fromUserId && c.ToUserId == loginUserId && !c.IsSeen && c.GroupId == 0).ToListAsync();
                if (result.Any())
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        result[i].IsSeen = true;
                    }
                    _context.Conversations.UpdateRange(result);
                    var affectedRow = await _context.SaveChangesAsync();
                    if (affectedRow > 0)
                    {
                        return new JsonResult("unread message updated");
                    }
                }
                return new JsonResult("Nothing unread message");
            }
            catch (Exception)
            {
                return new JsonResult("unread message not updated");
            }
        }

        [HttpGet]
        public async Task<int> GetUnreadMessageNo(string userId)
        {
            var loginUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _context.Conversations.Where(c => c.FromUserId == userId && c.ToUserId == loginUserId && !c.IsSeen && c.GroupId == 0).ToListAsync();
            return result.Count;
        }


        #region Chat Group

        [HttpGet]
        public async Task<IActionResult> OnGetGroupCreateOrEdit(int id = 0)
        {
            if (id == 0)
            {
                ChatGroup model = new();
                return PartialView("_CreateOrEditGroup", model);
            }
            else
            {
                var result = await _context.ChatGroups.FindAsync(id);
                if (result == null)
                    return null;
                return PartialView("_CreateOrEditGroup", result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEditGroup(ChatGroup chatGroup)
        {
            try
            {
                if (chatGroup.Id == 0)
                {
                    chatGroup.AuthorId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    chatGroup.CreatedDate = DateTime.Now;
                    chatGroup.IsDeleted = false;
                    var result = await _context.ChatGroups.AddAsync(chatGroup);
                    var affectedRow = await _context.SaveChangesAsync();
                    if (affectedRow > 0)
                    {
                        await _context.UserGroups.AddAsync(new UserGroup { Id = 0, GroupId = result.Entity.Id, UserId = chatGroup.AuthorId });
                        await _context.SaveChangesAsync();
                        return new JsonResult(new { IsValid = true, Id = result.Entity.Id, Name = result.Entity.Name });
                    }
                }
                else
                {
                    var result = await _context.ChatGroups.FindAsync(chatGroup.Id);
                    if (result == null)
                    {
                        return new JsonResult(new { IsValid = false });
                    }
                    else
                    {
                        result.UpdatedDate = DateTime.Now;
                        result.Name = chatGroup.Name;
                        result.Description = chatGroup.Description;
                        _context.ChatGroups.Update(result);
                        await _context.SaveChangesAsync();
                        return new JsonResult(new { IsValid = true, Id = chatGroup.Id, Name = chatGroup.Name });
                    }
                }
                return new JsonResult(new { IsValid = false });
            }
            catch
            {
                return new JsonResult(new { IsValid = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetCreateOrEdit(int groupId)
        {
            var chatGroup = await _context.ChatGroups.FindAsync(groupId);
            if (chatGroup == null)
            {
                return new JsonResult(new { IsValid = false });
            }
            GroupUsersViewModel model = new GroupUsersViewModel();
            var result = await _context.UserGroups.Where(g => g.GroupId == groupId).OrderBy(u => u.Id).ToListAsync();
            model.AuthorId = chatGroup.AuthorId;
            model.GroupName = chatGroup.Name;
            model.LoginUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserGroups = result;
            if (!result.Any())
            {
                model.UserGroups.Add(new UserGroup
                {
                    Id = 0,
                    GroupId = groupId,
                    UserId = string.Empty
                });
                return PartialView("_CreateOrEditGroupUsers", model);
            }
            else
            {
                return PartialView("_CreateOrEditGroupUsers", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostCreateOrEdit(List<UserGroup> userGroups)
        {
            try
            {
                var loginUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                userGroups.RemoveAll(g => g.GroupId == 0 && string.IsNullOrEmpty(g.UserId));
                if (userGroups.Any(p => p.Id == 0))
                {
                    foreach (var user in userGroups.Where(p => p.Id == 0))
                    {
                        user.CreatedDate = DateTime.Now;
                        user.CreatedBy = loginUserId;
                    }
                    var userList = userGroups.Where(p => p.Id == 0).Select(s => s.UserId).ToList();
                    await _context.UserGroups.AddRangeAsync(userGroups.Where(p => p.Id == 0));
                    var affectedRow = await _context.SaveChangesAsync();
                    if (affectedRow > 0)
                    {
                        //List<Notification> notifyList = new();
                        //foreach (var user in userList)
                        //{
                        //    Notification notification = new();
                        //    notification.Title = "Chat Notification";
                        //    notification.Text = $"Chat Notification From {User.Identity.Name}";
                        //    notification.NotifyUserId = loginUserId;
                        //    notification.ReceivedUserId = user;
                        //    notification.TransId = 10;
                        //    notification.Url = $"";
                        //    notifyList.Add(notification);
                        //    await _chatHubContext.Clients.User(user).SendAsync("ReceiveNotifications", notification);
                        //}
                        //await _context.AddRangeAsync(notifyList);
                        //await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    _context.UserGroups.UpdateRange(userGroups.Where(p => p.Id != 0));
                    await _context.SaveChangesAsync();
                }
                return new JsonResult(new { IsValid = true, Msg = "Users assign in this group" });
            }
            catch (Exception)
            {
                return new JsonResult(new { IsValid = false, Msg = "something went wrong" });
            }

        }

        [HttpGet]
        public async Task<IActionResult> LoadConversionsByGroupId(int groupId)
        {
            try
            {
                ConversationViewModel conversationViewModel = new ConversationViewModel();
                var group = await _context.ChatGroups.FindAsync(groupId);
                conversationViewModel.GroupName = group?.Name;
                conversationViewModel.GroupId = group.Id;
                conversationViewModel.GroupAuthorId = group.AuthorId;
                conversationViewModel.LoginUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var grpConversations = await _context.Conversations.Where(c => c.GroupId == groupId).ToListAsync();
                if (!grpConversations.Any())
                {
                    conversationViewModel.EmptyMessage = "Not Conversation Yet";
                }
                conversationViewModel.Conversations = grpConversations;
                return PartialView("_Conversations", conversationViewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostDeleteUserFromGroup(int id)
        {
            try
            {
                UserGroup result = await _context.UserGroups.FindAsync(id);
                if (result == null)
                {
                    return new JsonResult(new { IsValid = false, Msg = "User not found in this group" });
                }
                _context.UserGroups.Remove(result);
                var affectedRow = await _context.SaveChangesAsync();
                if (affectedRow > 0)
                {
                    //Notification notification = new();
                    //notification.Title = "Chat Notification";
                    //notification.Text = $"Chat Notification From {User.Identity.Name}";
                    //notification.NotifyUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    //await _chatHubContext.Clients.User(result.UserId).SendAsync("ReceiveNotifications", notification);
                }

                return new JsonResult(new { IsValid = true, Msg = "User is deleted from this group" });
            }
            catch (Exception)
            {

                throw;
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            try
            {
                var group = await _context.ChatGroups.FindAsync(groupId);
                if (group == null)
                    return new JsonResult(new { IsSuccess = false, Msg = "Group Not found" });
                group.IsDeleted = true;
                group.UpdatedDate = DateTime.Now;
                _context.ChatGroups.Update(group);
                await _context.SaveChangesAsync();
                return new JsonResult(new { IsSuccess = true, GroupName = group.Name });
            }
            catch (Exception)
            {
                return new JsonResult(new { IsSuccess = false, Msg = "Something went wrong" });
            }
        }

        #endregion

        [HttpGet]
        public async Task<List<ApplicationUser>> GetIdentityUsers() => await _userManager.Users.ToListAsync();

        private async Task<(string, string)> SaveImageAsync(IFormFile file, string fromUserId)
        {
            try
            {
                if (file is not null && file.Length > 0)                        // during create/changes file
                {
                    var fileSize = BytesToString(file.Length);
                    string uploadFolder = Path.Combine(_webHost.WebRootPath, "images", "users");
                    string extension = Path.GetExtension(file.FileName);
                    string fileName = $"{fromUserId}_{DateTime.Now.Millisecond.ToString()}{extension}";
                    string path = Path.Combine(uploadFolder, fileName);



                    using FileStream fileStream = new(path, FileMode.Create);
                    await file.CopyToAsync(fileStream);
                    fileStream.Position = 0;

                    return ($"/images/users/{fileName}", fileSize);
                }
                return (string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}
