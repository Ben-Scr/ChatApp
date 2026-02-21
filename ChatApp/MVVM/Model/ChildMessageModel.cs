namespace ChatClient.MVVM.Model
{
    public class ChatMessageModel
    {
        public string Content { get; set; } = string.Empty;
        public bool IsOwnMessage { get; set; }
    }
}