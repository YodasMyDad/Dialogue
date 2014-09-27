using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class MemberPoints : Entity
    {
        public MemberPoints()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public int Points { get; set; }
        public DateTime DateAdded { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public Guid? RelatedPostId { get; set; }
    }
}
