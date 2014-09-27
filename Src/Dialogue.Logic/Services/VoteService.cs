using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class VoteService
    {
        public Vote Add(Vote item)
        {
            ContextPerRequest.Db.Vote.Add(item);
            return item;
        }

        public Vote Get(Guid id)
        {
            return ContextPerRequest.Db.Vote.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Vote item)
        {
            ContextPerRequest.Db.Vote.Remove(item);
        }

        public IEnumerable<Vote> GetAllVotesByUser(int memberId)
        {
            return ContextPerRequest.Db.Vote.Where(x => x.VotedByMemberId == memberId);
        }

        public List<Vote> GetAllVotesForPosts(List<Guid> postids)
        {
            return ContextPerRequest.Db.Vote.AsNoTracking().Where(x => postids.Contains(x.Post.Id)).ToList();
        }
    }
}