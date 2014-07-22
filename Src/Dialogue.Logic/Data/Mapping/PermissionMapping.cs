using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    /// <summary>
    /// Permissions
    /// </summary>
    public class PermissionMapping : EntityTypeConfiguration<Permission>
    {
        public PermissionMapping()
        {
            ToTable("DialoguePermission");
            HasKey(x => x.Id);
        }
    }

    public class CategoryPermissionMapping : EntityTypeConfiguration<CategoryPermission>
    {
        public CategoryPermissionMapping()
        {
            ToTable("DialogueCategoryPermission");
            HasKey(x => x.Id);
            HasRequired(x => x.Permission);
            Ignore(x => x.Category);
            Ignore(x => x.MemberGroup);         
        }
    }
}