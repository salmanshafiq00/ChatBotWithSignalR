namespace ChatBotWithSignalR.Interface
{
    public interface IChatHub
    {
        Task SendToUserAsync(Conversation conversation);

    }
}
