using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class Vote : Entity
    {
        public Vote()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public virtual Post Post { get; set; }
        public int VotedByMemberId { get; set; }
        public Member VotedByMember { get; set; }
        public virtual DateTime DateVoted { get; set; }
    }
}
