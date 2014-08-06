using System;

namespace Dialogue.Logic.Models
{
    public partial class Favourite
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Guid PostId { get; set; }
        public Guid TopicId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}