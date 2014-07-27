using System;
using Dialogue.Logic.Interfaces.Badges;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Attributes;

namespace Badge.MemberForAYearBadge
{
    [Id("52284d2b-7ed6-4154-9ccc-3a7d99b18cca")]
    [Name("MemberForAYear")]
    [DisplayName("First Anniversary")]
    [Description("This badge is awarded to a user after their first year anniversary.")]
    [Image("MemberForAYearBadge.png")]
    [AwardsPoints(2)]
    public class MemberForAYearBadge : ITimeBadge
    {
        public bool Rule(Member user)
        {
            var anniversary = new DateTime(user.DateCreated.Year + 1, user.DateCreated.Month, user.DateCreated.Day);
            return DateTime.UtcNow >= anniversary;
        }
    }
}
