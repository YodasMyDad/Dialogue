namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data.Context;
    using Models;

    public class FavouriteService : IRequestCachedService
    {
        public Favourite Add(Favourite dialogueFavourite)
        {
            return ContextPerRequest.Db.DialogueFavourite.Add(dialogueFavourite);
        }

        public Favourite Delete(Favourite dialogueFavourite)
        {
            return ContextPerRequest.Db.DialogueFavourite.Remove(dialogueFavourite);
        }

        public List<Favourite> GetAll()
        {
            return ContextPerRequest.Db.DialogueFavourite.ToList();
        }

        public List<Favourite> GetAllByMember(int memberId)
        {
            return ContextPerRequest.Db.DialogueFavourite.Where(x => x.MemberId == memberId).ToList();
        }

        public Favourite GetByMemberAndPost(int memberId, Guid postId)
        {
            return ContextPerRequest.Db.DialogueFavourite
                            .FirstOrDefault(x => x.MemberId == memberId && x.PostId == postId);
        }
    }
}