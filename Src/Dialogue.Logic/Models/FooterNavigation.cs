using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public partial class FooterNavigation
    {
        public string CurrentNodeContentPath { get; set; }
        public IList<IPublishedContent> Pages { get; set; } 
    }
}