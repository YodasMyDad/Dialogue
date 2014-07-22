using System;
using System.Collections.Generic;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class Permission : Entity
    {
        public Permission()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public partial class PermissionSet : Dictionary<string, CategoryPermission>
    {
        public PermissionSet(IEnumerable<CategoryPermission> permissionsList)
        {
            foreach (var categoryPermissionForRole in permissionsList)
            {
                Add(categoryPermissionForRole.Permission.Name, categoryPermissionForRole);
            }
        }
    }
}