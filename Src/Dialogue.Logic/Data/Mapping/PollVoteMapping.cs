using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class PollVoteMapping : EntityTypeConfiguration<PollVote>
    {
        public PollVoteMapping()
        {
            ToTable("DialoguePollVote");
            HasKey(x => x.Id);
            HasRequired(x => x.PollAnswer).WithMany(t => t.PollVotes).Map(m => m.MapKey("DialoguePollAnswer_Id"));
            Ignore(x => x.Member);
        }
    }
}
