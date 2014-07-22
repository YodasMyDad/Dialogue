using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class BannedEmailMapping : EntityTypeConfiguration<BannedEmail>
    {
        public BannedEmailMapping()
        {
            ToTable("DialogueBannedEmail");
            HasKey(x => x.Id);
        }
    }
}