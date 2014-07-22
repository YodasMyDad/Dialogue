using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class CategoryNotificationMapping : EntityTypeConfiguration<CategoryNotification>
    {
        public CategoryNotificationMapping()
        {
            ToTable("DialogueCategoryNotification");
            HasKey(x => x.Id);
            Ignore(x => x.Member);
            Ignore(x => x.Category);
        }
    }
}
