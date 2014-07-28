using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;
using Dialogue.Logic.Services;

namespace Badge.Padawan
{
    [Id("A88C62B2-394F-4D89-B61E-04A7B546416B")]
    [Name("PadawanBadge")]
    [DisplayName("Padawan")]
    [Description("Had 10 or more posts successfully marked as an answer.")]
    [Image("padawan.png")]
    [AwardsPoints(10)]
    public class PosterMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        public bool Rule(Member user)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var usersSolutions = ServiceFactory.PostService.GetSolutionsByMember(user.Id);

            return (usersSolutions.Count >= 10);
        }
    }
}
