using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Dialogue.Logic.Models
{
    public partial class DialogueForum : MasterModel
    {
        public DialogueForum() : base(UmbracoContext.Current.PublishedContentRequest.PublishedContent)
        {
        }

        public string MainHeading { get; set; }
        public string MainContent { get; set; }
        public List<Category> MainCategories { get; set; }
    }
}