using System;
using System.Collections.Generic;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models
{
    public enum PostOrderBy
    {
        Standard,
        Newest,
        Votes,
        All
    }

    public partial class Post : Entity
    {
        public Post()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public string PostContent { get; set; }
        public DateTime DateCreated { get; set; }
        public int VoteCount { get; set; }
        public DateTime? DateEdited { get; set; }
        public bool IsSolution { get; set; }
        public bool IsTopicStarter { get; set; }
        public bool FlaggedAsSpam { get; set; }
        public string IpAddress { get; set; }
        public bool Pending { get; set; }
        public virtual Topic Topic { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public virtual IList<Vote> Votes { get; set; }
        public virtual IList<UploadedFile> Files { get; set; }
    }
}
