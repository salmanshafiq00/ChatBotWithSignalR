using ChatBotWithSignalR.DTOs;

namespace ChatBotWithSignalR.Interface
{
    public interface IMailService
    {
        Task<bool> SendAsync(MailRequest mailRequest, CancellationToken cancellation);
    }
}
