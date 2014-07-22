using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Mapping
{
    public static class PrivateMessageMapper
    {
        /// <summary>
        /// Extension method to complete the PrivateMessage model with full Members (To and From)
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public static List<PrivateMessage> CompleteModels(this List<PrivateMessage> entityList)
        {
            // Get all ids and map into members
            var membersIds = entityList.Select(x => x.MemberToId).ToList();
            membersIds.AddRange(entityList.Select(x => x.MemberFromId));
            var members = MemberMapper.MapMember(membersIds.Distinct().ToList());

            // Mapp all TO members
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberToId);
                entity.MemberTo = member;
            }

            //Map all FROM members
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberFromId);
                entity.MemberFrom = member;
            }

            return entityList;
        }
    }
}