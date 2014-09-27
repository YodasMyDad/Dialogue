using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;

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

        public static ViewPostViewModel MapPostViewModel(PermissionSet permissions, 
                                                            Post post, Member currentMember, 
                                                                DialogueSettings settings, Topic topic,
                                                                    List<Vote> allPostVotes, List<Favourite> favourites, bool showTopicLinks = false)
        {
            var postViewModel = new ViewPostViewModel
            {
                Permissions = permissions,
                Post = post,
                User = currentMember,
                ParentTopic = topic,
                Votes = allPostVotes.Where(x => x.Post.Id == post.Id).ToList(),
                LoggedOnMemberId = currentMember != null ? currentMember.Id : 0,
                AllowedToVote = (currentMember != null && currentMember.Id != post.MemberId &&
                                 currentMember.TotalPoints > settings.AmountOfPointsBeforeAUserCanVote),
                PostCount = post.Member.PostCount,
                IsAdminOrMod = HttpContext.Current.User.IsInRole(AppConstants.AdminRoleName) || permissions[AppConstants.PermissionModerate].IsTicked,
                HasFavourited = favourites.Any(x => x.PostId == post.Id),
                IsTopicStarter = post.IsTopicStarter,
                ShowTopicLinks = showTopicLinks
            };
            postViewModel.UpVotes = postViewModel.Votes.Count(x => x.Amount > 0);
            postViewModel.DownVotes = postViewModel.Votes.Count(x => x.Amount < 0);
            return postViewModel;
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