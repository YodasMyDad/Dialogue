using System.Collections.Generic;

namespace Dialogue.Logic.Models.ViewModels
{
    public class BreadCrumbViewModel
    {
        public List<Category> Categories { get; set; }
        public Topic Topic { get; set; }
        public Category Category { get; set; }
    }
}