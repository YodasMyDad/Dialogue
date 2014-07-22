using System;
using System.Collections.Generic;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public enum BadgeType
    {
        VoteUp,
        VoteDown,
        MarkAsSolution,
        Time,
        Post
    }

    public partial class Badge : Entity
    {
        public Badge()
        {
            Id = AppHelpers.GenerateComb();
        }

        /// <summary>
        /// Specifies the target badge interface names matched to the corresponding badge type
        /// </summary>
        public static readonly Dictionary<BadgeType, string> BadgeClassNames = new Dictionary<BadgeType, string>
                                                            {
                                                                {BadgeType.VoteUp, "MVCForum.Domain.Interfaces.Badges.IVoteUpBadge"},
                                                                {BadgeType.MarkAsSolution, "MVCForum.Domain.Interfaces.Badges.IMarkAsSolutionBadge"},
                                                                {BadgeType.Time, "MVCForum.Domain.Interfaces.Badges.ITimeBadge"},
                                                                {BadgeType.Post, "MVCForum.Domain.Interfaces.Badges.IPostBadge"},
                                                                {BadgeType.VoteDown, "MVCForum.Domain.Interfaces.Badges.IVoteDownBadge"}
                                                            };

        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Milestone { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int? AwardsPoints { get; set; }
        public IList<Member> Members { get; set; }

    }

    public partial class BadgeToMember : Entity
    {
        public int Id { get; set; }
        public Guid DialogueBadgeId { get; set; }
        public int MemberId { get; set; }
    }
}
