using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class BadgeMapping : EntityTypeConfiguration<Badge>
    {
        public BadgeMapping()
        {
            ToTable("DialogueBadge");
            HasKey(x => x.Id);
            Ignore(x => x.Milestone);
            Ignore(x => x.Members);
        }
    }

    public class BadgeToMemberMapping : EntityTypeConfiguration<BadgeToMember>
    {
        public BadgeToMemberMapping()
        {
            ToTable("DialogueMember_Badge");
            HasKey(x => x.Id);
            Property(t => t.DialogueBadgeId).HasColumnName("DialogueBadgeId");
        }
    }
}
