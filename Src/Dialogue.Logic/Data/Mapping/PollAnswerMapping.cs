using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class PollAnswerMapping : EntityTypeConfiguration<PollAnswer>
    {
        public PollAnswerMapping()
        {
            ToTable("DialoguePollAnswer");
            HasKey(x => x.Id);
            HasRequired(x => x.Poll).WithMany(t => t.PollAnswers).Map(m => m.MapKey("DialoguePoll_Id"));
            HasMany(x => x.PollVotes)
                .WithRequired(t => t.PollAnswer)
                .Map(x => x.MapKey("DialoguePollAnswer_Id"))
                .WillCascadeOnDelete();
        }
    }
}
