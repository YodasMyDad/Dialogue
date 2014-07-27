using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class VoteBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class MarkAsSolutionBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class PostBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class TimeBadgeViewModel
    {
        public int Id { get; set; }
    }

    public class AllBadgesViewModel : MasterModel
    {
        public AllBadgesViewModel(IPublishedContent content)
            : base(content)
        {
        }

        public IList<Badge> AllBadges { get; set; }
    }
}