using System;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Controllers
{
    //public partial class DialogueBadgeController : BaseController
    //{
    //    private readonly CategoryService _categoryService;
    //    private readonly TopicService _topicService;
    //    private readonly CategoryNotificationService _categoryNotificationService;
    //    private readonly IMemberGroup _usersRole;
    //    private readonly BadgeService _badgeService;

    //    public DialogueBadgeController()
    //    {
    //        _categoryService = new CategoryService();
    //        _topicService = new TopicService();
    //        _categoryNotificationService = new CategoryNotificationService();
    //        _usersRole = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
    //        _badgeService = new BadgeService();
    //    }



    //}

    public partial class DialogueBadgeSurfaceController : BaseSurfaceController
    {
        private readonly IMemberGroup _usersRole;
        private readonly CategoryService _categoryService;
        private readonly PostService _postService;
        private readonly BadgeService _badgeService;

        public DialogueBadgeSurfaceController()
        {
            _usersRole = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
            _categoryService = new CategoryService();
            _postService = new PostService();
            _badgeService = new BadgeService();
        }

        [HttpPost]
        [Authorize]
        public void VoteUpPost(VoteBadgeViewModel voteUpBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var databaseUpdateNeededOne = _badgeService.ProcessBadge(BadgeType.VoteUp, CurrentMember);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = _postService.Get(voteUpBadgeViewModel.PostId);
                    var member = MemberService.Get(post.MemberId);
                    var databaseUpdateNeededTwo = _badgeService.ProcessBadge(BadgeType.VoteUp, member);
                    if (databaseUpdateNeededTwo)
                    {
                        unitOfwork.SaveChanges();
                    }

                    if (databaseUpdateNeededOne || databaseUpdateNeededTwo)
                    {
                        unitOfwork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LogError(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void VoteDownPost(VoteBadgeViewModel voteUpBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var databaseUpdateNeededOne = _badgeService.ProcessBadge(BadgeType.VoteDown, CurrentMember);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = _postService.Get(voteUpBadgeViewModel.PostId);
                    var member = MemberService.Get(post.MemberId);
                    var databaseUpdateNeededTwo = _badgeService.ProcessBadge(BadgeType.VoteDown, member);

                    if (databaseUpdateNeededTwo)
                    {
                        unitOfwork.SaveChanges();
                    }

                    if (databaseUpdateNeededOne || databaseUpdateNeededTwo)
                    {
                        unitOfwork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LogError(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void Post()
        {
            if (Request.IsAjaxRequest())
            {
                using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Post, CurrentMember);

                        if (databaseUpdateNeeded)
                        {
                            unitOfwork.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        unitOfwork.Rollback();
                        LogError(ex);
                    }
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void MarkAsSolution(MarkAsSolutionBadgeViewModel markAsSolutionBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var post = _postService.Get(markAsSolutionBadgeViewModel.PostId);
                    var postMember = MemberService.Get(post.MemberId);
                    var topicMember = MemberService.Get(post.Topic.MemberId);
                    var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.MarkAsSolution, postMember) | _badgeService.ProcessBadge(BadgeType.MarkAsSolution, topicMember);

                    if (databaseUpdateNeeded)
                    {
                        unitOfwork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LogError(ex);
                }
            }
        }

        [HttpPost]
        public void Time(TimeBadgeViewModel timeBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var user = MemberService.Get(timeBadgeViewModel.Id);
                    var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Time, user);

                    if (databaseUpdateNeeded)
                    {
                        unitOfwork.Commit();
                    }

                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LogError(ex);
                }
            }
        }

    }
}