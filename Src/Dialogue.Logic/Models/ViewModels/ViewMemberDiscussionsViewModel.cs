using System.Collections.Generic;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ViewMemberDiscussionsViewModel
    {
        public IList<Topic> Topics { get; set; }
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public Member CurrentUser { get; set; }
    }
}