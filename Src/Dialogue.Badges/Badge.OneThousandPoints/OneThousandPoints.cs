using System.Linq;
using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;
using Dialogue.Logic.Services;

namespace Badge.OneThousandPoints
{
    [Id("a54ec5d1-111d-4698-b2d0-78fbdaa52d1b")]
    [Name("OneThousandPoints")]
    [DisplayName("Thousand Pointer")]
    [Description("This badge is awarded to users who have received 1000 points.")]
    [Image("OneThousandPoints.png")]
    [AwardsPoints(10)]
    public class OneThousandPoints : IPostBadge
    {
        public bool Rule(Member user)
        {
            var points = user.Points.Sum(x => x.Points);
            return points >= 1000;
        }
    }
}

