﻿using ChatBotWithSignalR.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatBotWithSignalR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
