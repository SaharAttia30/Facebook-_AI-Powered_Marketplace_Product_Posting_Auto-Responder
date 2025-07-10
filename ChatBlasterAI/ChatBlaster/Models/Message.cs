namespace ChatBlaster.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public DateTime SentAt { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public string ConversationId { get; set; }  // Foreign key to Conversation
        public virtual Conversation Conversation { get; set; }
    }
}
