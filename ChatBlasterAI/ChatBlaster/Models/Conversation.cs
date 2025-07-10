using System.ComponentModel.DataAnnotations.Schema;
using OpenQA.Selenium;

namespace ChatBlaster.Models
{
    public class Conversation
    {
        public string ConversationId { get; set; }
        public string _conversationLink { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public string AvatarId { get; set; }
        public virtual Avatar Avatar { get; set; }
        [NotMapped]
        public bool _hasUnreadMessages { get; set; }
        [NotMapped]
        public string _userName { get; set; }
        [NotMapped]
        public DateTime _lastSeen { get; set; }
        [NotMapped]
        public string _lastMessage { get; set; }
        [NotMapped]
        public IWebElement _WebDriveElement { get; set; }
        public Conversation()
        {
            Messages = new List<Message>();
        }
        public Conversation(Avatar avatar, string conversationId, IWebElement web_drive_element, string conversationLink, string conversationType,
                bool hasUnreadMessages, string userName, string lastMessageContent, DateTime lastMessageTime)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty.", nameof(conversationId));
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("User name cannot be null or empty.", nameof(userName));
            if (string.IsNullOrEmpty(lastMessageContent))
                throw new ArgumentException("Last message content cannot be null or empty.", nameof(lastMessageContent));
            _WebDriveElement = web_drive_element;
            _conversationLink = conversationLink;
            ConversationId = conversationId;
            _hasUnreadMessages = hasUnreadMessages;
            _userName = userName;
            _lastSeen = lastMessageTime;
            _lastMessage = lastMessageContent;
            AvatarId = avatar.Id;
            Avatar = avatar;
        }

    }
}
