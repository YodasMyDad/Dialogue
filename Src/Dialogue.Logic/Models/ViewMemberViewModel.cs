using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public class ViewMemberViewModel : MasterModel
    {
        public ViewMemberViewModel(IPublishedContent content) : base(content)
        {
        }
        public Member User { get; set; }
        public int PostCount { get; set; }
        public int LoggedOnUserId { get; set; }
    }
}