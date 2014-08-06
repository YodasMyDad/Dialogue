using System;

namespace Dialogue.Logic.Models.ViewModels
{
    public class VoteViewModel
    {
        public Guid Post { get; set; }
        public bool IsVoteUp { get; set; }
    }

    public class MarkAsSolutionViewModel
    {
        public Guid Post { get; set; }
    }
}