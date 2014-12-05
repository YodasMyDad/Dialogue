using System;
using System.Linq;
using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;
using Dialogue.Logic.Services;

namespace Badge.PostMentionUmbraco
{
    [Id("9a247d50-35b5-4cd2-adaa-a0cf013325ac")]
    [Name("PostContainsUmbraco")]
    [DisplayName("Umbraco Fan")]
    [Description("This badge is awarded to a user who mentions Umbraco in their latest post.")]
    [Image("MentionsUmbracoBadge.png")]
    [AwardsPoints(1)]
    public class PostMentionsUmbraco : IPostBadge
    {
        public bool Rule(Member user)
        {
            var usersPosts = ServiceFactory.PostService.GetByMember(user.Id);


            var lastPost = usersPosts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
            if (lastPost != null && lastPost.PostContent.IndexOf("umbraco", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
            return false;
        }
    }
}
