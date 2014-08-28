using System.Collections.Generic;
using Umbraco.Web;

namespace Dialogue.Logic.Models
{
    public class SpamOverviewModel : MasterModel
    {
        //TODO - Double check this works as expected, instead of passing in current page manually
        public SpamOverviewModel()
            : base(UmbracoContext.Current.PublishedContentRequest.PublishedContent)
        {

        }

        public Dictionary<Member, int> DodgyMembers { get; set; }
        public IList<Post> DodgyPosts { get; set; }
    }
}