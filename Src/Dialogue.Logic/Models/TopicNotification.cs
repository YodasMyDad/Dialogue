using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class TopicNotification : Entity
    {
        public TopicNotification()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual Topic Topic { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
