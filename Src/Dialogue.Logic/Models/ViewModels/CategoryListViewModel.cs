using System.Collections.Generic;

namespace Dialogue.Logic.Models.ViewModels
{
    public class CategoryListViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
    }
}