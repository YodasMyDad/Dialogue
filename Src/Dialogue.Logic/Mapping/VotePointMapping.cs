using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Mapping
{
    public static class VotePointMapping
    {
        #region Votes

        /// <summary>
        /// Extension method to complete the Vote model with full Members (inc Voted By members)
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public static List<Vote> CompleteModels(this List<Vote> entityList)
        {
            // Get all ids and map into members
            var membersIds = entityList.Select(x => x.MemberId).ToList();
            membersIds.AddRange(entityList.Select(x => x.VotedByMemberId));
            var members = MemberMapper.MapMember(membersIds.Distinct().ToList());

            // Mapp all members
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberId);
                entity.Member = member;
            }

            //Map all Voted By members
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.VotedByMemberId);
                entity.VotedByMember = member;
            }

            return entityList;
        }

        #endregion
    }
}