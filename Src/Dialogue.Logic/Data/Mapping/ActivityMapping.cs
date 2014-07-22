using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models.Activity;

namespace Dialogue.Logic.Data.Mapping
{
    public class ActivityMapping : EntityTypeConfiguration<Activity>
    {
        public ActivityMapping()
        {
            ToTable("DialogueActivity");
            HasKey(x => x.Id);            
        }
    }
}