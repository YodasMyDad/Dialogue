using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ApproveTopicViewModel
    {
        public Guid Id { get; set; }
    }

    public class CreateTopicViewModel
    {
        [Required]
        [StringLength(600)]
        [DialogueDisplayName("Topic.Label.TopicTitle")]
        public string TopicName { get; set; }

        [UIHint(AppConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string TopicContent { get; set; }

        public bool IsSticky { get; set; }

        public bool IsLocked { get; set; }

        [Required]
        [DialogueDisplayName("Topic.Label.Category")]
        public int Category { get; set; }

        public List<PollAnswer> PollAnswers { get; set; }

        [DialogueDisplayName("Topic.Label.SubscribeToTopic")]
        public bool SubscribeToTopic { get; set; }

    }

    public class CreateTopicButtonViewModel
    {
        public Member LoggedOnUser { get; set; }
        public bool UserCanPostTopics { get; set; }
        public int CategoryId { get; set; }
    }

    public class ShowTopicViewModel : MasterModel
    {
        public ShowTopicViewModel(IPublishedContent content)
            : base(content)
        {
        }

        public Topic Topic { get; set; }
        public List<ViewPostViewModel> Posts { get; set; }
        public List<Favourite> Favourites { get; set; }
        public Member User { get; set; }
        public ViewPostViewModel TopicStarterPost { get; set; }
        public PermissionSet Permissions { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool IsSubscribed { get; set; }
        public bool UserHasAlreadyVotedInPoll { get; set; }
        public int TotalVotesInPoll { get; set; }
        public string PostContent { get; set; }
    }

    public class ActiveTopicsViewModel
    {
        public PagedList<Topic> Topics { get; set; }
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public Member User { get; set; }
    }

    public class TagTopicsViewModel
    {
        public PagedList<Topic> Topics { get; set; }
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }

        public string Tag { get; set; }

        public Member User { get; set; }
    }


    public class GetMorePostsViewModel
    {
        public Guid TopicId { get; set; }
        public int PageIndex { get; set; }
        public string Order { get; set; }
    }

    public class ShowMorePostsViewModel
    {
        public List<ViewPostViewModel> Posts { get; set; }
        public PermissionSet Permissions { get; set; }
        public Member User { get; set; }
        public Topic Topic { get; set; }
    }

    public class ShowPollViewModel
    {
        public Poll Poll { get; set; }
        public bool UserHasAlreadyVoted { get; set; }
        public int TotalVotesInPoll { get; set; }
        public bool UserAllowedToVote { get; set; }
    }

    public class UpdatePollViewModel
    {
        public Guid PollId { get; set; }
        public Guid AnswerId { get; set; }
    }

    public class ViewTopicViewModel
    {
        public Topic Topic { get; set; }
        public PermissionSet Permissions { get; set; }
        public Member User { get; set; }
        public bool ShowCategoryName { get; set; }
    }

    public class MoveTopicViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class NotifyNewTopicViewModel
    {
        public Guid CategoryId { get; set; }
    }



    public class SearchViewModel : MasterModel
    {
        public SearchViewModel(IPublishedContent content) : base(content)
        {
        }

        public List<ViewPostViewModel> Posts { get; set; }
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public string Term { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}