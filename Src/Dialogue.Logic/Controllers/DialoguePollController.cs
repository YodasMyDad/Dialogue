using System;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.Controllers
{
    public partial class DialoguePollSurfaceController : BaseSurfaceController
    {
        [HttpPost]
        public PartialViewResult UpdatePoll(UpdatePollViewModel updatePollViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // Fist need to check this user hasn't voted already and is trying to fudge the system
                    if (!ServiceFactory.PollService.HasUserVotedAlready(updatePollViewModel.AnswerId, CurrentMember.Id))
                    {
                        // Get the answer
                        var pollAnswer = ServiceFactory.PollService.GetAnswer(updatePollViewModel.AnswerId);

                        // create a new vote
                        var pollVote = new PollVote { PollAnswer = pollAnswer, Member = CurrentMember, MemberId = CurrentMember.Id};

                        // Add it
                        ServiceFactory.PollService.Add(pollVote);

                        // Update the context so the changes are reflected in the viewmodel below
                        unitOfWork.SaveChanges();
                    }

                    // Create the view model and get ready return the poll partial view
                    var poll = ServiceFactory.PollService.Get(updatePollViewModel.PollId);
                    var votes = poll.PollAnswers.SelectMany(x => x.PollVotes).ToList();
                    var alreadyVoted = (votes.Count(x => x.MemberId == CurrentMember.Id) > 0);
                    var viewModel = new ShowPollViewModel { Poll = poll, TotalVotesInPoll = votes.Count(), UserHasAlreadyVoted = alreadyVoted };

                    // Commit the transaction
                    unitOfWork.Commit();

                    return PartialView(PathHelper.GetThemePartialViewPath("Poll"), viewModel);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LogError(ex);
                    throw new Exception(Lang("Errors.GenericMessage"));
                }
            }
        }
    }
}