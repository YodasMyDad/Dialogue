using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class PollMapping : EntityTypeConfiguration<Poll>
    {
        public PollMapping()
        {
            ToTable("DialoguePoll");
            HasKey(x => x.Id);
            HasMany(x => x.PollAnswers)
                .WithRequired(t => t.Poll)
                .Map(x => x.MapKey("DialoguePoll_Id"))
                .WillCascadeOnDelete();
            Ignore(x => x.Member);
        }
    }
}
