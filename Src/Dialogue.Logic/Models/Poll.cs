using System;
using System.Collections.Generic;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class Poll : Entity
    {
        public Poll()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public bool IsClosed { get; set; }
        public DateTime DateCreated { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public virtual IList<PollAnswer> PollAnswers { get; set; } 
    }
}
