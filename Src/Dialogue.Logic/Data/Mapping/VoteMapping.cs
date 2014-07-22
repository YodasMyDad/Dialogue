using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class VoteMapping : EntityTypeConfiguration<Vote>
    {
        public VoteMapping()
        {
            ToTable("DialogueVote");
            HasKey(x => x.Id);
            Ignore(x => x.Member);
            HasRequired(x => x.Post).WithMany(x => x.Votes).Map(x => x.MapKey("DialoguePost_Id"));
            Ignore(x => x.VotedByMember);          
        }
    }
}
