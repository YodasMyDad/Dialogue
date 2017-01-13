namespace Dialogue.Logic.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Application;
    using Models;
    using Models.ViewModels;

    public partial class DialogueVoteController : DialogueBaseController
    {
        [HttpPost]
        [Authorize]
        public ActionResult PostVote(VoteViewModel voteUpViewModel)
        {
            if (Request.IsAjaxRequest())
            {

                // Quick check to see if user is locked out, when logged in
                if (CurrentMember.IsLockedOut | !CurrentMember.IsApproved)
                {
                    MemberService.LogOff();
                    throw new Exception(Lang("Errors.NoAccess"));
                }


                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // Firstly get the post
                    var post = PostService.Get(voteUpViewModel.Post);

                    var allowedToVote = (CurrentMember.Id != post.MemberId &&
                                    CurrentMember.TotalPoints > Settings.AmountOfPointsBeforeAUserCanVote &&
                                    post.Votes.All(x => x.MemberId != CurrentMember.Id));

                    if (allowedToVote)
                    {
                        // Now get the current user
                        var voter = CurrentMember;

                        // Also get the user that wrote the post
                        var postWriter = MemberService.Get(post.MemberId);

                        // Mark the post up or down
                        string returnValue;
                        if (voteUpViewModel.IsVoteUp)
                        {
                            returnValue = MarkPostUpOrDown(post, postWriter, voter, PostType.Positive);
                        }
                        else
                        {
                            returnValue = MarkPostUpOrDown(post, postWriter, voter, PostType.Negative);
                        }

                        try
                        {
                            unitOfWork.Commit();
                            return Content(returnValue);
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LogError(ex);
                        }
                    }
                    else
                    {
                        return Content(post.VoteCount.ToString());
                    }

                }
            }
            throw new Exception(Lang("Errors.GenericMessage"));
        }

        private string MarkPostUpOrDown(Post post, Member postWriter, Member voter, PostType postType)
        {
            // Check this user is not the post owner
            if (voter.Id != postWriter.Id)
            {
                // Not the same person, now check they haven't voted on this post before
                if (post.Votes.All(x => x.MemberId != CurrentMember.Id))
                {

                    // Points to add or subtract to a user
                    var usersPoints = (postType == PostType.Negative) ?
                                        (-Settings.PointsDeductedForNegativeVote) : (Settings.PointsAddedForPositiveVote);

                    // Update the users points who wrote the post
                    MemberPointsService.Add(new MemberPoints
                    {
                        Points = usersPoints, 
                        Member = postWriter, 
                        MemberId = postWriter.Id,
                        RelatedPostId = post.Id
                    });

                    // Update the post with the new vote of the voter
                    var vote = new Vote
                    {
                        Post = post,
                        Member = voter,
                        MemberId = voter.Id,
                        Amount = (postType == PostType.Negative) ? (-1) : (1),
                        VotedByMember = CurrentMember,
                        DateVoted = DateTime.Now
                    };
                    VoteService.Add(vote);

                    // Update the post with the new points amount
                    var allVotes = post.Votes.ToList();
                    var allVoteCount = allVotes.Sum(x => x.Amount);
                    //var newPointTotal = (postType == PostType.Negative) ? (post.VoteCount - 1) : (post.VoteCount + 1);
                    post.VoteCount = allVoteCount;
                    int postTypeVoteCount;
                    if (postType == PostType.Positive)
                    {
                        postTypeVoteCount = allVotes.Count(x => x.Amount > 0);
                    }
                    else
                    {
                        postTypeVoteCount =  allVotes.Count(x => x.Amount < 0);   
                    }
                    return string.Concat(postTypeVoteCount, ",", allVoteCount);
                }
            }
            return "0";
        }

        private enum PostType
        {
            Positive,
            Negative,
        };

        [HttpPost]
        [Authorize]
        public ActionResult MarkAsSolution(MarkAsSolutionViewModel markAsSolutionViewModel)
        {
            if (Request.IsAjaxRequest())
            {

                // Quick check to see if user is locked out, when logged in
                if (CurrentMember.IsLockedOut | !CurrentMember.IsApproved)
                {
                    MemberService.LogOff();
                    throw new Exception(Lang("Errors.NoAccess"));
                }


                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // Firstly get the post
                    var post = PostService.Get(markAsSolutionViewModel.Post);

                    // Check the member marking owns the topic
                    if (CurrentMember.Id == post.Topic.MemberId)
                    {
                        // Person who created the solution post
                        var solutionWriter = MemberService.Get(post.MemberId);

                        // Get the post topic
                        var topic = post.Topic;

                        // Now get the current user
                        var marker = CurrentMember;
                        try
                        {
                            var solved = TopicService.SolveTopic(topic, post, marker, solutionWriter, MemberPointsService);

                            if (solved)
                            {
                                unitOfWork.Commit();
                                return Content($"<span class=\"glyphicon glyphicon-ok\"></span> {Lang("Post.Solution")}");
                            }
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LogError(ex);
                            throw new Exception(Lang("Errors.GenericMessage"));
                        }
                    }
                    else
                    {
                        throw new Exception(Lang("Errors.Generic"));
                    }

                }
            }
            return null;
        }


        //[HttpPost]
        //public PartialViewResult GetVoters(VoteUpViewModel voteUpViewModel)
        //{
        //    if (Request.IsAjaxRequest())
        //    {
        //        var post = _postService.Get(voteUpViewModel.Post);
        //        var positiveVotes = post.Votes.Where(x => x.Amount > 0);
        //        var viewModel = new ShowVotersViewModel { Votes = positiveVotes.ToList() };
        //        return PartialView(viewModel);
        //    }
        //    return null;
        //}

        //[HttpPost]
        //public PartialViewResult GetVotes(VoteUpViewModel voteUpViewModel)
        //{
        //    if (Request.IsAjaxRequest())
        //    {
        //        var post = _postService.Get(voteUpViewModel.Post);
        //        var positiveVotes = post.Votes.Count(x => x.Amount > 0);
        //        var negativeVotes = post.Votes.Count(x => x.Amount <= 0);
        //        var viewModel = new ShowVotesViewModel { DownVotes = negativeVotes, UpVotes = positiveVotes };
        //        return PartialView(viewModel);
        //    }
        //    return null;
        //}
    }
}