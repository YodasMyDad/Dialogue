using System.Linq;
using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;
using Dialogue.Logic.Services;

namespace Badge.UserVoteUpBadge
{
    [Id("c9913ee2-b8e0-4543-8930-c723497ee65c")]
    [Name("UserVoteUp")]
    [DisplayName("You've Given Your First Vote Up")]
    [Description("This badge is awarded to users after they make their first vote up.")]
    [Image("UserVoteUpBadge.png")]
    [AwardsPoints(2)]
    public class UserVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(Member user)
        {
            var userVotes = ServiceFactory.VoteService.GetAllVotesByUser(user.Id).ToList();
            return userVotes.Count >= 1;
        }
    }
}
