using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class EditPermissionsViewModel
    {
        public CategoryViewModel Category { get; set; }
        public List<Permission> Permissions { get; set; }
        public List<string> CurrentPermissions { get; set; }
        public List<IMemberGroup> Groups { get; set; }
        public string GuestGroupName { get; set; }
        public Dictionary<int, Dictionary<Permission, bool>> FullPermissionTable { get; set; }
    }
}