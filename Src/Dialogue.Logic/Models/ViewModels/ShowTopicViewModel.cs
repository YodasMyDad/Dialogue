using Dialogue.Logic.Application;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ShowTopicViewModel : MasterModel
    {
        public ShowTopicViewModel(IPublishedContent content) : base(content)
        {
        }

        public Topic Topic { get; set; }
        public PagedList<Post> Posts { get; set; }
        public Member CurrentMember { get; set; }
    }
}