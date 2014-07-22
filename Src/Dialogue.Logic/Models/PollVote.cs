using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class PollVote
    {
        public PollVote()
        {
            Id = AppHelpers.GenerateComb();
        }

        public Guid Id { get; set; }
        public virtual PollAnswer PollAnswer { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
