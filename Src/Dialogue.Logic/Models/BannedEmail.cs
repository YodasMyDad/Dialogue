using System;

namespace Dialogue.Logic.Models
{
    public partial class BannedEmail : Entity
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime DateAdded { get; set; }
    }
}