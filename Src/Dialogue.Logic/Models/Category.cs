using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public partial class Category : MasterModel
    {
        public Category(IPublishedContent content) 
            : base(content)
        {
        }  
        public string Description { get; set; }
        public string Image { get; set; }

        // Settings
        public bool LockCategory { get; set; }
        public bool ModerateAllTopicsInThisCategory { get; set; }
        public bool ModerateAllPostsInThisCategory { get; set; }
        public List<Category> SubCategories { get; set; }
        public List<Category> ParentCategories { get; set; }
    }
}