using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Mapping
{
    public static class BadgeMapper
    {
        /// <summary>
        /// Adds the members to the list
        /// </summary>
        /// <returns></returns>
        public static List<BadgeTypeTimeLastChecked> BadgeTypeTimeLastChecked(List<BadgeTypeTimeLastChecked> entityList)
        {
            var membersIds = entityList.Select(x => x.MemberId).ToList();
            var members = MemberMapper.MapMember(membersIds);
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberId);
                entity.Member = member;
            }
            return entityList;
        } 
    }    
}