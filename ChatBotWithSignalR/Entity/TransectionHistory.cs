using ChatBotWithSignalR.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatBotWithSignalR.Entity
{
    public class TransectionHistory
    {
        public int Id { get; set; }
        public int TransId { get; set; }
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;
        [StringLength(500)]
        public string Text { get; set; } = string.Empty;
        [StringLength(50)]
        public string NotifyUserId { get; set; } = string.Empty;
        [NotMapped]
        [StringLength(50)]
        public string NotifyUserName { get; set; } = string.Empty;
        [StringLength(50)]
        public string ReceivedUserId { get; set; } = string.Empty;
        [NotMapped]
        [StringLength(50)]
        public string ReceivedUserName { get; set; } = string.Empty;
        public DateTime DateWithTime { get; set; } = DateTime.Now;
        public TransectionType Type { get; set; }
        public string? Url { get; set; } = string.Empty;
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        public bool IsSeen { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? SeenDate { get; set; }
    }
}
