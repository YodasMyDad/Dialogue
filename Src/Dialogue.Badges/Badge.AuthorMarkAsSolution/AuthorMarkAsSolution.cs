using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;
using Dialogue.Logic.Services;

namespace Badge.AuthorMarkAsSolution
{
    [Id("d68c289a-e3f7-4f55-ae4f-fc7ac2147781")]
    [Name("AuthorMarkAsSolution")]
    [DisplayName("Your Question Solved")]
    [Description("This badge is awarded to topic authors the first time they have a post marked as the answer.")]
    [Image("UserMarkAsSolutionBadge.png")]
    [AwardsPoints(2)]
    public class AuthorMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        public bool Rule(Member user)
        {
            return ServiceFactory.TopicService.GetSolvedTopicsByMember(user.Id).Count >= 1;
        }
    }
}
