using System;
using System.Web.Mvc;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.Controllers
{

    #region Surface Controllers

    public partial class DialogueBadgeSurfaceController : BaseSurfaceController
    {
        [HttpPost]
        [Authorize]
        public void VoteUpPost(VoteBadgeViewModel voteUpBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var databaseUpdateNeededOne = ServiceFactory.BadgeService.ProcessBadge(BadgeType.VoteUp, CurrentMember);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = ServiceFactory.PostService.Get(voteUpBadgeViewModel.PostId);
                    var member = ServiceFactory.MemberService.Get(post.MemberId);
                    var databaseUpdateNeededTwo = ServiceFactory.BadgeService.ProcessBadge(BadgeType.VoteUp, member);
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
                    var databaseUpdateNeededOne = ServiceFactory.BadgeService.ProcessBadge(BadgeType.VoteDown, CurrentMember);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = ServiceFactory.PostService.Get(voteUpBadgeViewModel.PostId);
                    var member = ServiceFactory.MemberService.Get(post.MemberId);
                    var databaseUpdateNeededTwo = ServiceFactory.BadgeService.ProcessBadge(BadgeType.VoteDown, member);

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
                        var databaseUpdateNeeded = ServiceFactory.BadgeService.ProcessBadge(BadgeType.Post, CurrentMember);

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
                    var post = ServiceFactory.PostService.Get(markAsSolutionBadgeViewModel.PostId);
                    var postMember = ServiceFactory.MemberService.Get(post.MemberId);
                    var topicMember = ServiceFactory.MemberService.Get(post.Topic.MemberId);
                    var databaseUpdateNeeded = ServiceFactory.BadgeService.ProcessBadge(BadgeType.MarkAsSolution, postMember) | ServiceFactory.BadgeService.ProcessBadge(BadgeType.MarkAsSolution, topicMember);

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
                    var user = ServiceFactory.MemberService.Get(timeBadgeViewModel.Id);
                    var databaseUpdateNeeded = ServiceFactory.BadgeService.ProcessBadge(BadgeType.Time, user);

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
    #endregion
}