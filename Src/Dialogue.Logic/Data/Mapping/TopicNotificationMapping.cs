using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class TopicNotificationMapping : EntityTypeConfiguration<TopicNotification>
    {
        public TopicNotificationMapping()
        {
            ToTable("DialogueTopicNotification");
            HasKey(x => x.Id);
            Ignore(x => x.Member);
            HasRequired(x => x.Topic).WithMany(x => x.TopicNotifications).Map(x => x.MapKey("DialogueTopic_Id"));
        }
    }
}
