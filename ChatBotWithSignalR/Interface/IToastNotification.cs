using Newtonsoft.Json;

namespace ChatBotWithSignalR.Interface
{
    public interface IToastNotification
    {
        Task ToastSuccess(string message);
        Task ToastError(string message);
        Task ToastWarning(string message);
        Task ToastInfo(string message);

    }
}
