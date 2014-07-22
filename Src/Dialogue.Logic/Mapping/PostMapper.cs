using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Mapping
{
    public static class PostMapper
    {
        #region Posts
        /// <summary>
        /// Extension method to complete the Post model with full Members
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public static List<Post> CompleteModels(this List<Post> entityList)
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

        #region Uploaded Files

        /// <summary>
        /// Extension method to complete the UploadedFile model with full Members
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public static List<UploadedFile> CompleteModels(this List<UploadedFile> entityList)
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