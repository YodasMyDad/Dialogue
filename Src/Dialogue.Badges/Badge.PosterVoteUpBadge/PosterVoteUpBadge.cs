using System.Linq;
using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;
using Dialogue.Logic.Services;

namespace Badge.PosterVoteUpBadge
{
    [Id("2ac1fc11-2f9e-4d5a-9df4-29715f10b6d1")]
    [Name("PosterVoteUp")]
    [DisplayName("First Vote Up Received")]
    [Description("This badge is awarded to users after they receive their first vote up from another user.")]
    [Image("PosterVoteUpBadge.png")]
    [AwardsPoints(2)]
    public class PosterVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(Member user)
        {
            var usersPosts = ServiceFactory.PostService.GetByMember(user.Id);
            return usersPosts != null && usersPosts.Any(post => post.Votes.Count > 0);
        }
    }
}
