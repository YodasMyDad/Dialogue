using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class BadgeTypeTimeLastChecked : Entity
    {
        public BadgeTypeTimeLastChecked()
        {
            Id = AppHelpers.GenerateComb();
        }

        public Guid Id { get; set; }
        public string BadgeType { get; set; }
        public DateTime TimeLastChecked { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
