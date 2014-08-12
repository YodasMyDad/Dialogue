using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public partial class Topic
    {
        public Topic()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Solved { get; set; }
        public string Slug { get; set; }
        public int Views { get; set; }
        public bool IsSticky { get; set; }
        public bool IsLocked { get; set; }
        public virtual Post LastPost { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public virtual IList<Post> Posts { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public virtual IList<TopicNotification> TopicNotifications { get; set; }
        public virtual Poll Poll { get; set; }
        public bool Pending { get; set; }
        public string Url
        {
            get
            {
                return Urls.GenerateUrl(Urls.UrlType.Topic, Slug);
            }
        }

        public int VoteCount
        {
            get
            {
                // Add a per request check here and cache
                return Posts.Select(x => x.VoteCount).Sum();
            }
        }
    }
}
