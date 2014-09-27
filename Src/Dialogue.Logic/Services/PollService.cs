using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class PollService
    {
        #region Polls

        public List<Poll> GetAllPolls()
        {
            return ContextPerRequest.Db.Poll.ToList();
        }

        public List<Poll> GetMembersPolls(int memberId)
        {
            return ContextPerRequest.Db.Poll.Where(x => x.MemberId == memberId).ToList();
        }

        public Poll Add(Poll poll)
        {
            poll.DateCreated = DateTime.UtcNow;
            poll.IsClosed = false;
            return ContextPerRequest.Db.Poll.Add(poll);
        }

        public Poll Get(Guid id)
        {
            return ContextPerRequest.Db.Poll.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Poll item)
        {
            ContextPerRequest.Db.Poll.Remove(item);
        } 

        #endregion

        #region Poll Answers

        public List<PollAnswer> GetAllPollAnswers()
        {
            return ContextPerRequest.Db.PollAnswer.ToList();
        }

        public PollAnswer Add(PollAnswer pollAnswer)
        {
            pollAnswer.Answer = AppHelpers.SafePlainText(pollAnswer.Answer);
            return ContextPerRequest.Db.PollAnswer.Add(pollAnswer);
        }

        public PollAnswer GetAnswer(Guid id)
        {
            return ContextPerRequest.Db.PollAnswer.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(PollAnswer pollAnswer)
        {
            ContextPerRequest.Db.PollAnswer.Remove(pollAnswer);
        }

        #endregion

        #region Poll Votes

        public List<PollVote> GetAllPollVotes()
        {
            return ContextPerRequest.Db.PollVote.ToList();
        }

        public PollVote Add(PollVote pollVote)
        {
            return ContextPerRequest.Db.PollVote.Add(pollVote);
        }

        public bool HasUserVotedAlready(Guid answerId, int userId)
        {
            var vote = ContextPerRequest.Db.PollVote.AsNoTracking().Where(x => x.PollAnswer.Id == answerId && x.MemberId == userId).Include(x => x.PollAnswer).FirstOrDefault();
            return (vote != null);
        }

        public PollVote GetPollVote(Guid id)
        {
            return ContextPerRequest.Db.PollVote.FirstOrDefault(x => x.Id == id);
        }

        public List<PollVote> GetMembersPollVotes(int memberId)
        {
            return ContextPerRequest.Db.PollVote.Where(x => x.MemberId == memberId).ToList();
        }

        public void Delete(PollVote pollVote)
        {
            ContextPerRequest.Db.PollVote.Remove(pollVote);
        }

        #endregion
    }
}