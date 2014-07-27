using Dialogue.Logic.Application;
using Dialogue.Logic.Models.Activity;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class AllRecentActivitiesViewModel : MasterModel
    {
        public AllRecentActivitiesViewModel(IPublishedContent content) : base(content)
        {
        }

        public PagedList<ActivityBase> Activities { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }
}