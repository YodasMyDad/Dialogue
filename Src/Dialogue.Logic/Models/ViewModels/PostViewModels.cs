using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ApprovePostViewModel
    {
        public Guid Id { get; set; }
    }

    public class CreateAjaxPostViewModel
    {
        [UIHint(AppConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string PostContent { get; set; }
        public Guid Topic { get; set; }
        public bool DisablePosting { get; set; }
    }

    public class ViewPostViewModel
    {
        public Post Post { get; set; }
        public List<Vote> Votes { get; set; }
        public Topic ParentTopic { get; set; }
        public PermissionSet Permissions { get; set; }
        public Member User { get; set; }
        public int LoggedOnMemberId { get; set; }
        public bool AllowedToVote { get; set; }
        public int PostCount { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public bool IsAdminOrMod { get; set; }
        public bool HasFavourited { get; set; }
        public bool IsTopicStarter { get; set; }
        public bool ShowTopicLinks { get; set; }
    }

    public class EditPostPageViewModel : MasterModel
    {
        public EditPostPageViewModel(IPublishedContent content) : base(content)
        {
        }

        public EditPostViewModel EditPostViewModel { get; set; }
    }

    public class EditPostViewModel
    {
        [DialogueDisplayName("Post.Label.TopicName")]
        [Required]
        [StringLength(600)]
        public string Name { get; set; }

        [DialogueDisplayName("Post.Label.IsStickyTopic")]
        public bool IsSticky { get; set; }

        [DialogueDisplayName("Post.Label.LockTopic")]
        public bool IsLocked { get; set; }

        [Required]
        [DialogueDisplayName("Post.label.TopicCategory")]
        public int Category { get; set; }

        public string Tags { get; set; }

        public IList<PollAnswer> PollAnswers { get; set; }

        public IEnumerable<Category> Categories { get; set; }

        [UIHint(AppConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string Content { get; set; }

        [HiddenInput]
        public Guid Id { get; set; }

        public bool IsTopicStarter { get; set; }

        public PermissionSet Permissions { get; set; }
    }

    public class ReportPostPageViewModel : MasterModel
    {
        public ReportPostPageViewModel(IPublishedContent content) : base(content)
        {
        }
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        public string PostCreatorUsername { get; set; }

        [Required]
        public string Reason { get; set; }

    }

    public class ReportPostViewModel
    {
        public Guid PostId { get; set; }
        public string PostCreatorUsername { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}