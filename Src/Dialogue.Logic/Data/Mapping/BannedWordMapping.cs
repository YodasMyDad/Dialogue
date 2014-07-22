using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class BannedWordMapping : EntityTypeConfiguration<BannedWord>
    {
        public BannedWordMapping()
        {
            ToTable("DialogueBannedWord");
            HasKey(x => x.Id);
        }
    }
}