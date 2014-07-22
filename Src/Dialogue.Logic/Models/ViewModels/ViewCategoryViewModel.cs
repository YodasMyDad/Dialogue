using Dialogue.Logic.Application;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ViewCategoryViewModel : MasterModel
    {
        public ViewCategoryViewModel(IPublishedContent content) : base(content)
        {

        }

        public PagedList<Topic> Topics { get; set; }
        public PermissionSet Permissions { get; set; }
        public Category Category { get; set; }
        public CategoryListViewModel SubCategories { get; set; }
        public Member User { get; set; }
        public bool IsSubscribed { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }

}