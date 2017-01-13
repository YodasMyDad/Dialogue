namespace Dialogue.Logic.Controllers
{
    using System;
    using System.Web.Mvc;
    using Models;
    using Models.ViewModels;
    using Services;

    public partial class DialogueBadgeController : DialogueBaseController
    {
        [HttpPost]
        [Authorize]
        public void VoteUpPost(VoteBadgeViewModel voteUpBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var databaseUpdateNeededOne = BadgeService.ProcessBadge(BadgeType.VoteUp, CurrentMember, MemberPointsService, ActivityService);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = PostService.Get(voteUpBadgeViewModel.PostId);
                    var member = MemberService.Get(post.MemberId);
                    var databaseUpdateNeededTwo = BadgeService.ProcessBadge(BadgeType.VoteUp, member, MemberPointsService, ActivityService);
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
                    var databaseUpdateNeededOne = BadgeService.ProcessBadge(BadgeType.VoteDown, CurrentMember, MemberPointsService, ActivityService);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = PostService.Get(voteUpBadgeViewModel.PostId);
                    var member = MemberService.Get(post.MemberId);
                    var databaseUpdateNeededTwo = BadgeService.ProcessBadge(BadgeType.VoteDown, member, MemberPointsService, ActivityService);

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
                        var databaseUpdateNeeded = BadgeService.ProcessBadge(BadgeType.Post, CurrentMember, MemberPointsService, ActivityService);

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
                    var post = PostService.Get(markAsSolutionBadgeViewModel.PostId);
                    var postMember = MemberService.Get(post.MemberId);
                    var topicMember = MemberService.Get(post.Topic.MemberId);
                    var databaseUpdateNeeded = BadgeService.ProcessBadge(BadgeType.MarkAsSolution, postMember, MemberPointsService, ActivityService) | 
                                                BadgeService.ProcessBadge(BadgeType.MarkAsSolution, topicMember, MemberPointsService, ActivityService);

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
                    var databaseUpdateNeeded = BadgeService.ProcessBadge(BadgeType.Time, user, MemberPointsService, ActivityService);

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