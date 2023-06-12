using ChatBotWithSignalR.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatBotWithSignalR.Views.Shared.Components.Notification
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationViewComponent(ApplicationDbContext context) => _context = context;
        public async Task<IViewComponentResult> InvokeAsync(string loginUserId)
        {
            var notifications = await _context.TransectionHistories.Where(x => x.NotifyUserId == loginUserId).ToListAsync();
            ViewBag.UnreadMessageCount = notifications.Where(x => !x.IsSeen).Count();
            return View(notifications);
        }
    }
}
