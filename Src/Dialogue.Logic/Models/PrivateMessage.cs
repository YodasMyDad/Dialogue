using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class PrivateMessage : Entity
    {
        public PrivateMessage()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public DateTime DateSent { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public bool IsSentMessage { get; set; }
        public int MemberToId { get; set; }
        public Member MemberTo { get; set; }
        public int MemberFromId { get; set; }
        public Member MemberFrom { get; set; }
    }
}
