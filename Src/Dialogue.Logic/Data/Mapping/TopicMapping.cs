using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class TopicMapping : EntityTypeConfiguration<Topic>
    {
        public TopicMapping()
        {
            ToTable("DialogueTopic");
            HasKey(x => x.Id);            

            // LastPost is not really optional but causes a circular dependency so needs to be added in after the main post is saved
            HasOptional(t => t.LastPost).WithOptionalDependent().Map(m => m.MapKey("DialoguePost_Id"));

            HasOptional(t => t.Poll).WithOptionalDependent().Map(m => m.MapKey("DialoguePoll_Id"));

            Ignore(x => x.Member);

            Ignore(x => x.Category);

            HasMany(x => x.Posts).WithRequired(x => x.Topic).Map(x => x.MapKey("DialogueTopic_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.TopicNotifications).WithRequired(x => x.Topic).Map(x => x.MapKey("DialogueTopic_Id"))
                .WillCascadeOnDelete();
        }
    }
}
