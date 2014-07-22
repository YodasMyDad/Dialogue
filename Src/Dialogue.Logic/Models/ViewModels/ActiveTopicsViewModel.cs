using System.Collections.Generic;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ActiveTopicsViewModel
    {
        public PagedList<Topic> Topics { get; set; }
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public Member User { get; set; }
    }
}