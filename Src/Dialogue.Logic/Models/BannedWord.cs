using System;

namespace Dialogue.Logic.Models
{
    public partial class BannedWord
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public DateTime DateAdded { get; set; }
    }
}