namespace Dialogue.Logic.Models.ViewModels
{
    public class ViewAdminSidePanelViewModel
    {
        public ViewAdminSidePanelViewModel()
        {
            CurrentUser = new Member();
        }
        public Member CurrentUser { get; set; }
        public int NewPrivateMessageCount { get; set; }
    }
}