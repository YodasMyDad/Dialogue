using System.Collections.Generic;

namespace Dialogue.Logic.Models.ViewModels
{
    public class MainStatsViewModel
    {
        public int PostCount { get; set; }
        public int TopicCount { get; set; }
        public int MemberCount { get; set; }
        public Dictionary<string, string> LatestMembers { get; set; }
    }
}