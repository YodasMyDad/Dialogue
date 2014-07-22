using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public class MainNavigation
    {
        public bool HomePageIsActive { get; set; }
        public string CurrentNodeContentPath { get; set; }
        public IList<IPublishedContent> MainPages { get; set; } 
    }
}