using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public class PollService
    {
        #region Polls

        public List<Poll> GetAllPolls()
        {
            return ContextPerRequest.Db.Poll.ToList();
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
    }
}