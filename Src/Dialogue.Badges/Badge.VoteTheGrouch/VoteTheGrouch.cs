namespace Badge.VoteTheGrouch
{
    using System.Linq;
    using Dialogue.Logic.Services;
    using Dialogue.Logic.Interfaces.Badges;
    using Dialogue.Logic.Models;
    using Dialogue.Logic.Models.Attributes;

    [Id("9ea3f651-ef37-4ad5-86a2-432012ad1e74")]
    [Name("TheGrouch")]
    [DisplayName("The Grouch")]
    [Description("This badge is awarded to users who have voted down other users posts 10 or more times.")]
    [Image("TheGrouch.png")]
    [AwardsPoints(0)]
    public class GrouchBadge : IVoteDownBadge
    {
        public bool Rule(Member user)
        {
            var voteService = new VoteService();

            // Get all down votes
            var downVotes = voteService.GetAllVotesByUser(user.Id).Where(x => x.Amount < 1).ToList();
            return downVotes.Count() >= 10;
        }
    }
}
