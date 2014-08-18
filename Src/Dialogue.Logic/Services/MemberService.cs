using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Web.Security;
using Member = Dialogue.Logic.Models.Member;

namespace Dialogue.Logic.Services
{
    public partial class MemberService
    {
        private readonly Umbraco.Core.Services.IMemberService _memberService;
        private readonly Umbraco.Core.Services.IMemberGroupService _memberGroupService;
        private readonly MembershipHelper _membershipHelper;
        public MemberService()
        {
            _memberService = AppHelpers.UmbServices().MemberService;
            _memberGroupService = AppHelpers.UmbServices().MemberGroupService;
            _membershipHelper = AppHelpers.UmbMemberHelper();
        }

        #region Members

        public IList<Member> GetActiveMembers()
        {
            // Get members that last activity date is valid
            var date = DateTime.UtcNow.AddMinutes(-AppConstants.TimeSpanInMinutesToShowMembers);
            var ids = _memberService.GetMembersByPropertyValue("lastActiveDate", date, ValuePropertyMatchType.GreaterThan)
                .Where(x => x.IsApproved && !x.IsLockedOut)
                .Select(x => x.Id);
            return MemberMapper.MapMember(ids.ToList());
        }

        public IEnumerable<string> GetMembersWithSameSlug(string slug)
        {
            // Get members that last activity date is valid
            var usernames = _memberService.GetMembersByPropertyValue(AppConstants.PropMemberSlug, slug, StringPropertyMatchType.StartsWith)
                .Select(x => x.Properties[AppConstants.PropMemberSlug].Value.ToString());
            return usernames;
        }

        public Member GetUserBySlug(string slug, bool getFullMember = false)
        {
            var safeSlug = AppHelpers.SafePlainText(slug);
            var key = string.Format("umb-member-{0}-{1}", safeSlug, getFullMember);
            if (!HttpContext.Current.Items.Contains(key))
            {
                var member = MemberMapper.MapMember(_memberService.GetMembersByPropertyValue(AppConstants.PropMemberSlug, slug).FirstOrDefault(), getFullMember);
                HttpContext.Current.Items.Add(key, member);
            }
            return HttpContext.Current.Items[key] as Member;
        }

        public Member Get(int id, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetById(id), populateFull);
        }

        public List<Member> GetUsersById(List<int> id, bool populateFull = false)
        {
            return MemberMapper.MapMember(id, populateFull);
        }

        public Member GetByEmail(string email, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetByEmail(email), populateFull);
        }

        public Member GetByUsername(string username, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetByUsername(username), populateFull);
        }

        public Member CurrentMember(bool populateFull = false)
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var key = string.Format("pub-memb-{0}-{1}", HttpContext.Current.User.Identity.Name, populateFull);
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, GetByUsername(HttpContext.Current.User.Identity.Name, populateFull));
                }
                return HttpContext.Current.Items[key] as Member;
            }

            return null;
        }

        public Member Get(string username, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetByUsername(username), populateFull);
        }

        public MembersPaged GetAll(int pageIndex, int pageSize, out int totalRecords)
        {
            var members = _memberService.GetAll(pageIndex, pageSize, out totalRecords);
            var mappedMembers = MemberMapper.MapMember(members.Select(x => x.Id).ToList());
            return MemberMapper.MapPagedMember(mappedMembers, pageIndex, pageSize, totalRecords);
        }

        public List<Member> GetAllById(List<int> ids, bool populateFull = false)
        {
            var members = _memberService.GetAllMembers(ids.ToArray());
            return MemberMapper.MapMember(members.Select(x => x.Id).ToList());
        }

        public MembersPaged Search(int pageIndex, int pageSize, string searchTerm, out int totalRecords)
        {
            var members = _memberService.GetAll(pageIndex, pageSize, out totalRecords)
                            .Where(x => x.Username.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
            var mappedMembers = MemberMapper.MapMember(members.Select(x => x.Id).ToList());
            return MemberMapper.MapPagedMember(mappedMembers, pageIndex, pageSize, totalRecords);
        }

        #region Deleting Member Stuff
        public bool Delete(Member member, UnitOfWork unitOfWork)
        {
            if (DeleteAllAssociatedMemberInfo(member.Id, unitOfWork))
            {
                var baseMember = _memberService.GetById(member.Id);
                _memberService.Delete(baseMember);
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method deletes/clears all member data, but not the actual member itself.
        /// Perfect for clearing spammers accounts before banning them. It needs a UnitOfWork passed in
        /// because it has to do a lot of saving and removing. so make sure you wrap it in a using statement
        /// NOTE: It calls it's own commit at the end of this method
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public bool DeleteAllAssociatedMemberInfo(int userId, UnitOfWork unitOfWork)
        {
            try
            {
                // Delete all file uploads
                var files = ServiceFactory.UploadedFileService.GetAllByUser(userId);
                var filesList = new List<UploadedFile>();
                filesList.AddRange(files);
                foreach (var file in filesList)
                {
                    // store the file path as we'll need it to delete on the file system
                    var filePath = file.FilePath;

                    // Now delete it
                    ServiceFactory.UploadedFileService.Delete(file);

                    // And finally delete from the file system
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(filePath));
                }

                // Delete all posts
                var posts = ServiceFactory.PostService.GetAllByMember(userId);
                var postList = new List<Post>();
                postList.AddRange(posts);
                foreach (var post in postList)
                {
                    post.Files.Clear();
                    ServiceFactory.PostService.Delete(post);
                }

                unitOfWork.SaveChanges();

                // Also clear their poll votes
                var userPollVotes = ServiceFactory.PollService.GetMembersPollVotes(userId);
                if (userPollVotes.Any())
                {
                    var pollList = new List<PollVote>();
                    pollList.AddRange(userPollVotes);
                    foreach (var vote in pollList)
                    {
                        ServiceFactory.PollService.Delete(vote);
                    }
                }

                unitOfWork.SaveChanges();

                // Also clear their polls
                var userPolls = ServiceFactory.PollService.GetMembersPolls(userId);
                if (userPolls.Any())
                {
                    var polls = new List<Poll>();
                    polls.AddRange(userPolls);
                    foreach (var poll in polls)
                    {
                        //Delete the poll answers
                        var pollAnswers = poll.PollAnswers;
                        if (pollAnswers.Any())
                        {
                            var pollAnswersList = new List<PollAnswer>();
                            pollAnswersList.AddRange(pollAnswers);
                            foreach (var answer in pollAnswersList)
                            {
                                answer.Poll = null;
                                ServiceFactory.PollService.Delete(answer);
                            }
                        }

                        poll.PollAnswers.Clear();
                        ServiceFactory.PollService.Delete(poll);
                    }
                }

                unitOfWork.SaveChanges();

                // Delete all topics
                var topics = ServiceFactory.TopicService.GetAllTopicsByUser(userId);
                var topicList = new List<Topic>();
                topicList.AddRange(topics);
                foreach (var topic in topicList)
                {
                    ServiceFactory.TopicService.Delete(topic);
                }

                // Now clear all activities for this user
                var usersActivities = ServiceFactory.ActivityService.GetDataByUserId(userId);
                ServiceFactory.ActivityService.Delete(usersActivities.ToList());


                // Delete all private messages from this user
                var msgsToDelete = new List<PrivateMessage>();
                msgsToDelete.AddRange(ServiceFactory.PrivateMessageService.GetAllByUserSentOrReceived(userId));
                foreach (var msgToDelete in msgsToDelete)
                {
                    ServiceFactory.PrivateMessageService.DeleteMessage(msgToDelete);
                }


                // Delete all badge times last checked
                var badgeTypesTimeLastCheckedToDelete = new List<BadgeTypeTimeLastChecked>();
                badgeTypesTimeLastCheckedToDelete.AddRange(ServiceFactory.BadgeService.BadgeTypeTimeLastCheckedByMember(userId));
                foreach (var badgeTypeTimeLastCheckedToDelete in badgeTypesTimeLastCheckedToDelete)
                {
                    ServiceFactory.BadgeService.DeleteTimeLastChecked(badgeTypeTimeLastCheckedToDelete);
                }

                // Delete all points from this user
                var pointsToDelete = new List<MemberPoints>();
                pointsToDelete.AddRange(ServiceFactory.MemberPointsService.GetByUser(userId));
                foreach (var pointToDelete in pointsToDelete)
                {
                    ServiceFactory.MemberPointsService.Delete(pointToDelete);
                }

                // Delete all topic notifications
                var topicNotificationsToDelete = new List<TopicNotification>();
                topicNotificationsToDelete.AddRange(ServiceFactory.TopicNotificationService.GetByUser(userId));
                foreach (var topicNotificationToDelete in topicNotificationsToDelete)
                {
                    ServiceFactory.TopicNotificationService.Delete(topicNotificationToDelete);
                }

                // Delete all user's votes
                var votesToDelete = new List<Vote>();
                votesToDelete.AddRange(ServiceFactory.VoteService.GetAllVotesByUser(userId));
                foreach (var voteToDelete in votesToDelete)
                {
                    ServiceFactory.VoteService.Delete(voteToDelete);
                }

                // Delete all user's badges
                var badgesToDelete = new List<BadgeToMember>();
                badgesToDelete.AddRange(ServiceFactory.BadgeService.GetAllBadgeToMembers(userId));
                foreach (var badgeToDelete in badgesToDelete)
                {
                    ServiceFactory.BadgeService.DeleteBadgeToMember(badgeToDelete);
                }

                // Delete all user's category notifications
                var categoryNotificationsToDelete = new List<CategoryNotification>();
                categoryNotificationsToDelete.AddRange(ServiceFactory.CategoryNotificationService.GetByUser(userId));
                foreach (var categoryNotificationToDelete in categoryNotificationsToDelete)
                {
                    ServiceFactory.CategoryNotificationService.Delete(categoryNotificationToDelete);
                }

                unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                AppHelpers.LogError("Error trying to delete Dialogue member", ex);
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Use this when banning a spammer
        /// They use signature and website fields for urls. This clears both
        /// </summary>
        /// <param name="member"></param>
        /// <param name="banMemberToo"></param>
        public void ClearWebsiteAndSignature(Member member, bool banMemberToo = false)
        {
            var baseMember = _memberService.GetById(member.Id);
            baseMember.Properties[AppConstants.PropMemberWebsite].Value = string.Empty;
            baseMember.Properties[AppConstants.PropMemberSignature].Value = string.Empty;
            if (banMemberToo)
            {
                baseMember.Properties[AppConstants.PropMemberUmbracoMemberLockedOut].Value = 1;
            }
            _memberService.Save(baseMember);
        }

        /// <summary>
        /// Saves a front end member
        /// </summary>
        /// <param name="member"></param>
        /// <param name="changedUsername"></param>
        public void SaveMember(Member member, bool changedUsername)
        {
            var baseMember = _memberService.GetById(member.Id);
            
            // Only change username if it's different
            if (changedUsername)
            {
                baseMember.Username = member.UserName;
                baseMember.Name = member.UserName;
            }

            baseMember.Properties[AppConstants.PropMemberEmail].Value = member.Email;            
            baseMember.Properties[AppConstants.PropMemberSignature].Value = member.Signature;
            baseMember.Properties[AppConstants.PropMemberWebsite].Value = member.Website;
            baseMember.Properties[AppConstants.PropMemberTwitter].Value = member.Twitter;
            baseMember.Properties[AppConstants.PropMemberAvatar].Value = member.Avatar;
            baseMember.Properties[AppConstants.PropMemberCanEditOtherUsers].Value = member.CanEditOtherMembers;
            baseMember.Properties[AppConstants.PropMemberDisableEmailNotifications].Value = member.DisableEmailNotifications;
            baseMember.Properties[AppConstants.PropMemberDisablePosting].Value = member.DisablePosting;
            baseMember.Properties[AppConstants.PropMemberDisablePrivateMessages].Value = member.DisablePrivateMessages;
            baseMember.Properties[AppConstants.PropMemberDisableFileUploads].Value = member.DisableFileUploads;
            baseMember.Properties[AppConstants.PropMemberUmbracoMemberComments].Value = member.Comments;

            _memberService.Save(baseMember);
        }

        public void BanMember(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            baseMember.Properties[AppConstants.PropMemberUmbracoMemberLockedOut].Value = 1;
            _memberService.Save(baseMember);
        }

        public void UnBanMember(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            baseMember.Properties[AppConstants.PropMemberUmbracoMemberLockedOut].Value = 0;
            _memberService.Save(baseMember);
        }

        public void UpdateLastActiveDate(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            baseMember.Properties[AppConstants.PropMemberLastActiveDate].Value = member.LastActiveDate;
            _memberService.Save(baseMember);
        }

        public void AddPostCount(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            var newPostCount = (member.PostCount + 1);
            baseMember.Properties[AppConstants.PropMemberPostCount].Value = newPostCount;
            _memberService.Save(baseMember);
        }

        public void RemovePostCount(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            if (member.PostCount > 0)
            {
                var newPostCount = (member.PostCount - 1);
                baseMember.Properties[AppConstants.PropMemberPostCount].Value = newPostCount;
                _memberService.Save(baseMember);
            }
        }

        public int MemberCount()
        {
            return _memberService.GetMembersByMemberType(AppConstants.MemberTypeAlias).Count(x => x.IsApproved && !x.IsLockedOut);
        }

        public bool Login(string username, string password)
        {
            return AppHelpers.UmbMemberHelper().Login(username, password);
        }

        public IList<Member> GetLatestUsers(int amountToTake)
        {
            var ids = _memberService.GetMembersByMemberType(AppConstants.MemberTypeAlias)
              .OrderByDescending(x => x.CreateDate)
              .Take(amountToTake)
              .Select(x => x.Id);
            return MemberMapper.MapMember(ids.ToList());
        }

        public void LogOff()
        {
            AppHelpers.UmbMemberHelper().Logout();
        }

        public string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return AppHelpers.Lang("Members.Errors.DuplicateUserName");

                case MembershipCreateStatus.DuplicateEmail:
                    return AppHelpers.Lang("Members.Errors.DuplicateEmail");

                case MembershipCreateStatus.InvalidPassword:
                    return AppHelpers.Lang("Members.Errors.InvalidPassword");

                case MembershipCreateStatus.InvalidEmail:
                    return AppHelpers.Lang("Members.Errors.InvalidEmail");

                case MembershipCreateStatus.InvalidAnswer:
                    return AppHelpers.Lang("Members.Errors.InvalidAnswer");

                case MembershipCreateStatus.InvalidQuestion:
                    return AppHelpers.Lang("Members.Errors.InvalidQuestion");

                case MembershipCreateStatus.InvalidUserName:
                    return AppHelpers.Lang("Members.Errors.InvalidUserName");

                case MembershipCreateStatus.ProviderError:
                    return AppHelpers.Lang("Members.Errors.ProviderError");

                case MembershipCreateStatus.UserRejected:
                    return AppHelpers.Lang("Members.Errors.UserRejected");

                default:
                    return AppHelpers.Lang("Members.Errors.Unknown");
            }
        }

        public bool ResetPassword(Member member, string newPassword)
        {
            try
            {
                var iMember = _memberService.GetById(member.Id);
                _memberService.SavePassword(iMember, newPassword);
                return true;
            }
            catch (Exception ex)
            {
                AppHelpers.LogError("ResetPassword()", ex);
                return false;
            }

        }
        #endregion

        #region Member Groups

        public IMemberGroup GetGroupByName(string groupName)
        {
            return _memberGroupService.GetByName(groupName);
        }

        public IEnumerable<IMemberGroup> GetAll()
        {
            return _memberGroupService.GetAll();
        }

        #endregion

    }
}