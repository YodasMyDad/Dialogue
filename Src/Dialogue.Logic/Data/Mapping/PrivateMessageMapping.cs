using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class PrivateMessageMapping : EntityTypeConfiguration<PrivateMessage>
    {
        public PrivateMessageMapping()
        {
            ToTable("DialoguePrivateMessage");
            HasKey(x => x.Id);
            Ignore(x => x.MemberTo);
            Ignore(x => x.MemberFrom);       
        }
    }
}
