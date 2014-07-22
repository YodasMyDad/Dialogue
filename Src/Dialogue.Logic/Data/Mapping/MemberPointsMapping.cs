using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class MemberPointsMapping : EntityTypeConfiguration<MemberPoints>
    {
        public MemberPointsMapping()
        {
            ToTable("DialogueMemberPoints");
            HasKey(x => x.Id);
            Ignore(x => x.Member);
        }
    }
}
