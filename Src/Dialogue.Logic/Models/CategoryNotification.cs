using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public partial class CategoryNotification : Entity
    {
        public CategoryNotification()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public int CategoryId { get; set; }
        public int MemberId { get; set; }
        public Category Category { get; set; }
        public Member Member { get; set; }
    }
}
