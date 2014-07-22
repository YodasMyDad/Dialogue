using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class PostMapping : EntityTypeConfiguration<Post>
    {
        public PostMapping()
        {
            ToTable("DialoguePost");
            HasKey(x => x.Id);
            Ignore(x => x.Member);
            HasMany(x => x.Votes).WithRequired(x => x.Post)
                .Map(x => x.MapKey("DialoguePost_Id"))
                .WillCascadeOnDelete();
        }
    }
}
