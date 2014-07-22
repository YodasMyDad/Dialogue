namespace Dialogue.Logic.Models.ViewModels
{
    public class CreateTopicButtonViewModel
    {
        public Member LoggedOnUser { get; set; }
        public bool UserCanPostTopics { get; set; }
    }
}