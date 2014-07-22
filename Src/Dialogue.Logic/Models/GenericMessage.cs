namespace Dialogue.Logic.Models
{
    public class GenericMessageViewModel
    {
        public GenericMessageViewModel()
        {
            MessageType = GenericMessages.Info;
        }
        public string Message { get; set; }
        public GenericMessages MessageType { get; set; }
    }

    public enum GenericMessages
    {
        Warning,
        Danger,
        Success,
        Info
    }
}