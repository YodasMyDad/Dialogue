using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Mapping
{
    public static class PollMapper
    {
        #region Poll Vote

        /// <summary>
        /// Extension method to complete the PollVote model with full Members
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public static List<PollVote> CompleteModels(this List<PollVote> entityList)
        {
            // Map full Members
            var membersIds = entityList.Select(x => x.MemberId).ToList();
            var members = MemberMapper.MapMember(membersIds);
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberId);
                entity.Member = member;
            }
            return entityList;
        } 

        #endregion

        #region Poll
        /// <summary>
        /// Extension method to complete the Poll model with full Members
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public static List<Poll> CompleteModels(this List<Poll> entityList)
        {
            // Map full Members
            var membersIds = entityList.Select(x => x.MemberId).ToList();
            var members = MemberMapper.MapMember(membersIds);
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberId);
                entity.Member = member;
            }
            return entityList;
        } 
        #endregion
    }
}