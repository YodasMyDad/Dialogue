using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class BadgeTypeTimeLastCheckedMapping : EntityTypeConfiguration<BadgeTypeTimeLastChecked>
    {
        public BadgeTypeTimeLastCheckedMapping()
        {
            ToTable("DialogueBadgeTypeTimeLastChecked");
            HasKey(x => x.Id);
            Ignore(x => x.Member);
        }
    }
}
