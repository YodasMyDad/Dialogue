using System.Collections.Generic;
using System.Linq;

namespace Dialogue.Logic.Models
{
    public class InstallerResult
    {
        public InstallerResult()
        {
            ResultItems = new List<ResultItem>();
        }

        public bool CompletedSuccessfully()
        {
            var getAllUncompleted = ResultItems.FirstOrDefault(x => x.CompletedSuccessfully == false);
            if (getAllUncompleted == null)
            {
                return true;
            }
            return false;
        }
        public List<ResultItem> ResultItems { get; set; } 
    }

    public class ResultItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CompletedSuccessfully { get; set; }
        public bool RequiresConfigUpdate { get; set; }
    }
}