namespace Dialogue.Logic.Models.ViewModels
{
    public class ViewTopicViewModel
    {
        public Topic Topic { get; set; }
        public PermissionSet Permissions { get; set; }
        public Member User { get; set; }
        public bool ShowCategoryName { get; set; }
    }
}