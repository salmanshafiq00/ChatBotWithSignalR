using ChatBotWithSignalR.Interface;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ChatBotWithSignalR.Service
{
    public class ToastNotification : IToastNotification
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        public ToastNotification(IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }
        public async Task ToastSuccess(string message)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);

            await Task.Delay(0);
            tempData["Message"] = JsonConvert.SerializeObject(new { Status = "Success", Message = $"{message}" });
        }
        public async Task ToastError(string message)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
            await Task.Delay(0);
            tempData["Message"] = JsonConvert.SerializeObject(new { Status = "Error", Message = $"{message}" });
        }
        public async Task ToastWarning(string message)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
            await Task.Delay(0);
            tempData["Message"] = JsonConvert.SerializeObject(new { Status = "Warning", Message = $"{message}" });
        }
        public async Task ToastInfo(string message)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
            await Task.Delay(0);
            tempData["Message"] = JsonConvert.SerializeObject(new { Status = "Info", Message = $"{message}" });
        }
    }
}
