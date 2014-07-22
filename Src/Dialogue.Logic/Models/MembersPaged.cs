using System.Collections.Generic;

namespace Dialogue.Logic.Models
{
    public class MembersPaged
    {
        public List<Member> Members { get; set; } 
        public int PageIndex { get; set; } 
        public int PageSize { get; set; }
        public int TotalRecords { get; set; } 
    }
}