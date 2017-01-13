using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Dialogue.Logic.Models
{
    public abstract class MasterModel : PublishedContentWrapped
    {
        protected MasterModel(IPublishedContent content)
            : base(content)
        {
        }

        public string PageTitle { get; set; }
        public string MetaDesc { get; set; }
        public bool HideFromNavigation { get; set; }
        public bool ShowInFooter { get; set; }
        public string ConversionCode { get; set; }
        public IList<int> NodePath { get; set; }

    }
}