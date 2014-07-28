using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;
using Dialogue.Logic.Services;

namespace Badge.PosterMarkAsSolution
{
    [Id("8250f9f0-84d2-4dff-b651-c3df9e12bf2a")]
    [Name("PosterMarkAsSolution")]
    [DisplayName("Post Selected As Answer")]
    [Description("This badge is awarded to the poster of a post marked as the topic answer, the first time they author an answer.")]
    [Image("PosterMarkAsSolutionBadge.png")]
    [AwardsPoints(2)]
    public class PosterMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        public bool Rule(Member user)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var usersSolutions = ServiceFactory.PostService.GetSolutionsByMember(user.Id);
            return (usersSolutions.Count >= 1);
        }
    }
}
