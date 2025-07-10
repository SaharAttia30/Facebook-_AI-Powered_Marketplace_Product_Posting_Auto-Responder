namespace ChatBlaster.Models
{
    public class ConversationQueue
    {
        public List<Conversation> _conversations;
        private readonly IComparer<Conversation> _comparer;
        public ConversationQueue(IEnumerable<Conversation> conversations)
        {
            _comparer = new ConversationComparer();
            _conversations = new List<Conversation>(conversations);
            _conversations.Sort(_comparer);
        }
        public ConversationQueue()
        {
            _comparer = new ConversationComparer();
            _conversations = new List<Conversation>();
        }
        public void Enqueue(Conversation conversation)
        {
            _conversations.Add(conversation);
            _conversations.Sort(_comparer);
        }
        public Conversation Dequeue()
        {
            if (_conversations.Count == 0)
                throw new InvalidOperationException("Queue is empty");
            var conversation = _conversations[0];
            _conversations.RemoveAt(0);
            return conversation;
        }
        public int Count => _conversations.Count;
    }
    public class ConversationComparer : IComparer<Conversation>
    {
        public int Compare(Conversation x, Conversation y)
        {
            if (x._hasUnreadMessages != y._hasUnreadMessages)
                return y._hasUnreadMessages.CompareTo(x._hasUnreadMessages);
            return x._lastSeen.CompareTo(y._lastSeen);
        }
    }
}
