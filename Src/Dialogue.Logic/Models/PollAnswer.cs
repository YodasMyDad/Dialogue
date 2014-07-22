using System;
using System.Collections.Generic;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class PollAnswer
    {
        public PollAnswer()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Answer { get; set; }
        public virtual Poll Poll { get; set; }
        public virtual IList<PollVote> PollVotes { get; set; }
    }
}
