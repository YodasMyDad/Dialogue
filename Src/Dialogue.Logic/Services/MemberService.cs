﻿namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Security;
    using Application;
    using Constants;
    using Data.Context;
    using Data.UnitOfWork;
    using Mapping;
    using Models;
    using Umbraco.Core.Models;
    using Umbraco.Core.Persistence.Querying;
    using Umbraco.Core.Services;
    using Umbraco.Web.Security;
    using Member = Models.Member;

    public partial class MemberService : IRequestCachedService
    {
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IDataTypeService _dataTypeService;
        private readonly MembershipHelper _membershipHelper;

        public MemberService()
        {
            _memberService = AppHelpers.UmbServices().MemberService;
            _memberGroupService = AppHelpers.UmbServices().MemberGroupService;
            _membershipHelper = AppHelpers.UmbMemberHelper();
            _memberTypeService = AppHelpers.UmbServices().MemberTypeService;
            _dataTypeService = AppHelpers.UmbServices().DataTypeService;
        }


        #region Members

        /// <summary>
        /// Gets the members upload path
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public string GetMemberUploadPath(int memberId)
        {
            var uploadFolderPath = HttpContext.Current.Server.MapPath(string.Concat(AppConstants.UploadFolderPath, memberId));
            if (!Directory.Exists(uploadFolderPath))
            {
                Directory.CreateDirectory(uploadFolderPath);
            }
            return uploadFolderPath;
        }

        /// <summary>
        /// Gets the ID of the members media folder
        /// </summary>
        /// <returns></returns>
        public int ConfirmMemberAvatarMediaFolder()
        {
            // We want to look for the 'Member Avatars' folder in the media section
            // If it doesn't exist then we create it
            var rootMediaId = -1;
            const string folderName = "Dialogue Members Avatars";

            // Media Service
            var ms = AppHelpers.UmbServices().MediaService;

            // Check main media folder
            var mediaFolder = ms.GetRootMedia().FirstOrDefault(x => x.Name == folderName);
            if (mediaFolder == null)
            {
                // Doesn't exist, so create it
                mediaFolder = ms.CreateMedia(folderName, rootMediaId, "Folder");
                ms.Save(mediaFolder);
            }

            // Set id
            rootMediaId = mediaFolder.Id;

            // Check current user has a folder
            var cUser = CurrentMember();
            if (cUser != null)
            {
                // Get the members folder
                var memberFolder = mediaFolder.Children().FirstOrDefault(x => x.Name == cUser.UserName);
                if (memberFolder == null)
                {
                    // Doesn't exist, so create it
                    memberFolder = ms.CreateMedia(cUser.UserName, mediaFolder.Id, "Folder");
                    ms.Save(memberFolder);
                }

                // reset id
                rootMediaId = memberFolder.Id;
            }

            return rootMediaId;
        }

        public IList<Member> GetActiveMembers()
        {
            // Get members that last activity date is valid
            var date = DateTime.UtcNow.AddMinutes(-DialogueConfiguration.Instance.TimeSpanInMinutesToShowMembers);
            var ids = _memberService.GetMembersByPropertyValue(AppConstants.PropMemberLastActiveDate, date, ValuePropertyMatchType.GreaterThan)
                .Where(x => x.IsApproved && !x.IsLockedOut)
                .Select(x => x.Id);
            return MemberMapper.MapMember(ids.ToList());
        }

        public List<Member> GetUnAuthorisedMembers()
        {
            // Get members that last activity date is valid
            var ids = _memberService.GetAllMembers()
                .Where(x => !x.IsApproved && !x.IsLockedOut)
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
            var key = $"umb-member-{safeSlug}-{getFullMember}";
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
                var key = $"pub-memb-{HttpContext.Current.User.Identity.Name}-{populateFull}";
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
        public bool Delete(Member member, UnitOfWork unitOfWork, UploadedFileService uploadedFileService, PostService postService,
            MemberPointsService memberPointsService, PollService pollService, TopicService topicService, TopicNotificationService topicNotificationService,
            ActivityService activityService, PrivateMessageService privateMessageService, BadgeService badgeService, VoteService voteService, CategoryNotificationService categoryNotificationService)
        {
            if (DeleteAllAssociatedMemberInfo(member.Id, unitOfWork, uploadedFileService, postService, memberPointsService, pollService, 
                topicService, topicNotificationService, activityService, privateMessageService, badgeService, voteService, categoryNotificationService))
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
        /// <param name="uploadedFileService"></param>
        /// <param name="postService"></param>
        /// <param name="memberPointsService"></param>
        /// <param name="pollService"></param>
        /// <param name="topicService"></param>
        /// <param name="topicNotificationService"></param>
        /// <param name="activityService"></param>
        /// <param name="privateMessageService"></param>
        /// <param name="badgeService"></param>
        /// <param name="voteService"></param>
        /// <param name="categoryNotificationService"></param>
        /// <returns></returns>
        public bool DeleteAllAssociatedMemberInfo(int userId, UnitOfWork unitOfWork, UploadedFileService uploadedFileService, PostService postService, 
            MemberPointsService memberPointsService, PollService pollService, TopicService topicService, TopicNotificationService topicNotificationService,
            ActivityService activityService, PrivateMessageService privateMessageService, BadgeService badgeService, VoteService voteService, CategoryNotificationService categoryNotificationService)
        {
            try
            {
                // Delete all file uploads
                var files = uploadedFileService.GetAllByUser(userId);
                var filesList = new List<UploadedFile>();
                filesList.AddRange(files);
                foreach (var file in filesList)
                {
                    // store the file path as we'll need it to delete on the file system
                    var filePath = file.FilePath;

                    // Now delete it
                    uploadedFileService.Delete(file);

                    // And finally delete from the file system
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(filePath));
                }

                // Delete all posts - exlcuding topic starters as they are about to be deleted
                var groupedPosts = postService.GetAllByMember(userId).Where(x => !x.IsTopicStarter).GroupBy(x => x.Topic);

                // Loop through all posts per topic
                foreach (var group in groupedPosts)
                {
                    var postList = new List<Post>();
                    postList.AddRange(group);

                    // The Topic
                    var topic = group.Key;

                    // The last post
                    var lastPost = group.Key.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();

                    // Loop through the posts
                    foreach (var post in postList)
                    {
                        post.Files.Clear();

                        if (lastPost != null && lastPost.Id == post.Id)
                        {
                            // Get the new last post and update the topic
                            topic.LastPost = topic.Posts.Where(x => x.Id != post.Id).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                        }

                        // Mark topic as not solved if the post we are deleting was the solution
                        if (topic.Solved && post.IsSolution)
                        {
                            topic.Solved = false;
                        }

                        // Remove this post from the topic so we can delete it without any errors
                        topic.Posts.Remove(post);

                        // Delete all the points the memeber who made this post has gained
                        memberPointsService.DeletePostPoints(post);

                        // now delete the post
                        ContextPerRequest.Db.Post.Remove(post);
                    }

                    unitOfWork.SaveChanges();
                }


                // Also clear their poll votes
                var userPollVotes = pollService.GetMembersPollVotes(userId);
                if (userPollVotes.Any())
                {
                    var pollList = new List<PollVote>();
                    pollList.AddRange(userPollVotes);
                    foreach (var vote in pollList)
                    {
                        pollService.Delete(vote);
                    }
                }

                unitOfWork.SaveChanges();

                // Also clear their polls
                var userPolls = pollService.GetMembersPolls(userId);
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
                                pollService.Delete(answer);
                            }
                        }

                        poll.PollAnswers.Clear();
                        pollService.Delete(poll);
                    }
                }

                unitOfWork.SaveChanges();

                // Delete all topics
                var topics = topicService.GetAllTopicsByUser(userId);
                var topicList = new List<Topic>();
                topicList.AddRange(topics);
                var memberIds = new List<int>();
                foreach (var topic in topicList)
                {
                    //var topicStarterPost = topic.Posts.FirstOrDefault(x => x.IsTopicStarter);
                    //postService.Delete(topicStarterPost, postService, , memberPointsService, topicNotificationService);

                    var postsToDelete = new List<Post>();
                    postsToDelete.AddRange(topic.Posts);
                    memberIds.AddRange(postsToDelete.Select(x => x.MemberId).Distinct());
                    foreach (var postFromTopic in postsToDelete)
                    {
                        postFromTopic.Files.Clear();

                        // Remove this post from the topic so we can delete it without any errors
                        topic.Posts.Remove(postFromTopic);

                        // Delete all the points the memeber who made this post has gained
                        memberPointsService.DeletePostPoints(postFromTopic);
                    }

                    if (topic.TopicNotifications != null)
                    {
                        var notificationsToDelete = new List<TopicNotification>();
                        notificationsToDelete.AddRange(topic.TopicNotifications);
                        foreach (var topicNotification in notificationsToDelete)
                        {
                            topicNotificationService.Delete(topicNotification);
                        }
                    }

                    ContextPerRequest.Db.Topic.Remove(topic);
                }

                // Sync the members post count. For all members who had a post deleted.
                var members = GetAllById(memberIds);
                SyncMembersPostCount(members);

                // Now clear all activities for this user
                var usersActivities = activityService.GetDataByUserId(userId);
                activityService.Delete(usersActivities.ToList());


                // Delete all private messages from this user
                var msgsToDelete = new List<PrivateMessage>();
                msgsToDelete.AddRange(privateMessageService.GetAllByUserSentOrReceived(userId));
                foreach (var msgToDelete in msgsToDelete)
                {
                    privateMessageService.DeleteMessage(msgToDelete);
                }


                // Delete all badge times last checked
                var badgeTypesTimeLastCheckedToDelete = new List<BadgeTypeTimeLastChecked>();
                badgeTypesTimeLastCheckedToDelete.AddRange(badgeService.BadgeTypeTimeLastCheckedByMember(userId));
                foreach (var badgeTypeTimeLastCheckedToDelete in badgeTypesTimeLastCheckedToDelete)
                {
                    badgeService.DeleteTimeLastChecked(badgeTypeTimeLastCheckedToDelete);
                }

                // Delete all points from this user
                var pointsToDelete = new List<MemberPoints>();
                pointsToDelete.AddRange(memberPointsService.GetByUser(userId));
                foreach (var pointToDelete in pointsToDelete)
                {
                    memberPointsService.Delete(pointToDelete);
                }

                // Delete all topic notifications
                var topicNotificationsToDelete = new List<TopicNotification>();
                topicNotificationsToDelete.AddRange(topicNotificationService.GetByUser(userId));
                foreach (var topicNotificationToDelete in topicNotificationsToDelete)
                {
                    topicNotificationService.Delete(topicNotificationToDelete);
                }

                // Delete all user's votes
                var votesToDelete = new List<Vote>();
                votesToDelete.AddRange(voteService.GetAllVotesByUser(userId));
                foreach (var voteToDelete in votesToDelete)
                {
                    voteService.Delete(voteToDelete);
                }

                // Delete all user's badges
                var badgesToDelete = new List<BadgeToMember>();
                badgesToDelete.AddRange(badgeService.GetAllBadgeToMembers(userId));
                foreach (var badgeToDelete in badgesToDelete)
                {
                    badgeService.DeleteBadgeToMember(badgeToDelete);
                }

                // Delete all user's category notifications
                var categoryNotificationsToDelete = new List<CategoryNotification>();
                categoryNotificationsToDelete.AddRange(categoryNotificationService.GetByUser(userId));
                foreach (var categoryNotificationToDelete in categoryNotificationsToDelete)
                {
                    categoryNotificationService.Delete(categoryNotificationToDelete);
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
        /// They use signature and website fields for urls. This clears both as well as banning them
        /// </summary>
        /// <param name="member"></param>
        /// <param name="banMemberToo"></param>
        public void KillSpammer(Member member, bool banMemberToo = false)
        {
            var baseMember = _memberService.GetById(member.Id);
            baseMember.Properties[AppConstants.PropMemberWebsite].Value = string.Empty;
            baseMember.Properties[AppConstants.PropMemberSignature].Value = string.Empty;
            baseMember.Properties[AppConstants.PropMemberUmbracoMemberLockedOut].Value = 1;
            baseMember.Properties[AppConstants.PropMemberPostCount].Value = 0;
            _memberService.Save(baseMember);
        }

        public void RefreshMemberPosts(Member member, int amount)
        {
            var baseMember = _memberService.GetById(member.Id);
            if (baseMember != null && baseMember.Properties.Contains(AppConstants.PropMemberPostCount))
            {
                baseMember.Properties[AppConstants.PropMemberPostCount].Value = amount;
                _memberService.Save(baseMember);
            }
        }

        public void SyncMembersPostCount(List<Member> members)
        {
            var memberIds = members.Select(x => x.Id);
            var memberPoints = ContextPerRequest.Db.Post.AsNoTracking().Where(x => memberIds.Contains(x.MemberId));
            foreach (var m in members)
            {
                var member = m;
                var mPoints = memberPoints.Count(x => x.MemberId == member.Id);
                RefreshMemberPosts(member, mPoints);
            }
        }

        public void ReducePostCount(Member member, int amount)
        {
            var baseMember = _memberService.GetById(member.Id);
            if (member.PostCount > 0)
            {
                baseMember.Properties[AppConstants.PropMemberPostCount].Value = (member.PostCount - amount);
                _memberService.Save(baseMember);   
            }
        }

        public void AddPostCount(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            var newPostCount = (member.PostCount + 1);
            baseMember.Properties[AppConstants.PropMemberPostCount].Value = newPostCount;
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

            baseMember.Email = member.Email;       
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

        public void ApproveMember(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            baseMember.Properties[AppConstants.PropMemberUmbracoMemberApproved].Value = 1;
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

        public int MemberCount()
        {
            return _memberService.GetMembersByMemberType(DialogueConfiguration.Instance.MemberTypeAlias).Count(x => x.IsApproved && !x.IsLockedOut);
        }

        public bool Login(string username, string password)
        {
            return AppHelpers.UmbMemberHelper().Login(username, password);
        }

        public IList<Member> GetLatestUsers(int amountToTake)
        {
            var ids = _memberService.GetMembersByMemberType(DialogueConfiguration.Instance.MemberTypeAlias)
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

        public bool MemberGroupExists(string name)
        {
            var mGroup = _memberGroupService.GetByName(name);
            return mGroup != null;
        }

        public IMemberGroup CreateMemberGroup(string name)
        {
            var group = new MemberGroup
            {
                Name = name
            };
            //group.AdditionalData.Add("test1", 123);
            //group.AdditionalData.Add("test2", "hello");
            _memberGroupService.Save(group);

            return group;
        }

        #endregion

        #region Member Types

        public bool MemberTypeExists(string memberType)
        {
            var mType = _memberTypeService.Get(memberType);
            return mType != null;
        }

        public IMemberType AddDialogueMemberType()
        {

            #region DataType Ids
            //-49	    Umbraco.TrueFalse
            //-51	    Umbraco.Integer
            //-87	    Umbraco.TinyMCEv3
            //-88	    Umbraco.Textbox
            //-89	    Umbraco.TextboxMultiple
            //-90	    Umbraco.UploadField
            //-92	    Umbraco.NoEdit
            //-36	    Umbraco.DateTime
            //-37	    Umbraco.ColorPickerAlias
            //-38	    Umbraco.FolderBrowser
            //-39	    Umbraco.DropDownMultiple
            //-40	    Umbraco.RadioButtonList
            //-41	    Umbraco.Date
            //-42	    Umbraco.DropDown
            //-43	    Umbraco.CheckBoxList
            //1034	    Umbraco.ContentPickerAlias
            //1035	    Umbraco.MediaPicker
            //1036	    Umbraco.MemberPicker
            //1040	    Umbraco.RelatedLinks
            //1041	    Umbraco.Tags
            //1045	    Umbraco.MultipleMediaPicker
            //1077	    Apt.PartialPicker
            //1089	    Umbraco.ImageCropper
            //1092	    Umbraco.TinyMCEv3
            //1103	    Our.Umbraco.FilePicker
            //1104	    Umbraco.MemberGroupPicker
            //1105	    Apt.CssPicker
            //1128	    Dialogue.ThemePicker
            //1132	    Dialogue.Permissions
            //1147	    Umbraco.MultipleTextstring 
            #endregion

            var label = _dataTypeService.GetDataTypeDefinitionById(-92);
            var upload = _dataTypeService.GetDataTypeDefinitionById(-90);
            var textstring = _dataTypeService.GetDataTypeDefinitionById(-88);
            var textboxMultiple = _dataTypeService.GetDataTypeDefinitionById(-89);
            var truefalse = _dataTypeService.GetDataTypeDefinitionById(-49);
            var numeric = _dataTypeService.GetDataTypeDefinitionById(-51);
            var datetime = _dataTypeService.GetDataTypeDefinitionById(-36);

            // Profile, Settings, Social Tokens

            // Create the member Type
            var memType = new MemberType(-1)
            {
                Alias = DialogueConfiguration.Instance.MemberTypeAlias,
                Name = "Dialogue Member"
            };

            // Create the Profile group/tab
            var profileGroup = new PropertyGroup
            {
                Name = "Profile",
            };
            profileGroup.PropertyTypes.Add(new PropertyType(label)
            {
                Alias = "email",
                Name = "Email",
                SortOrder = 0,
                Description = "This is a bit rubbish, but it's the only way to get the email from the new member service at the current time"
            });
            profileGroup.PropertyTypes.Add(new PropertyType(upload)
            {
                Alias = "avatar",
                Name = "Avatar",
                SortOrder = 0,
                Description = ""
            });
            profileGroup.PropertyTypes.Add(new PropertyType(textstring)
            {
                Alias = "website",
                Name = "Website",
                SortOrder = 0,
                Description = ""
            });
            profileGroup.PropertyTypes.Add(new PropertyType(textstring)
            {
                Alias = "twitter",
                Name = "Twitter",
                SortOrder = 0,
                Description = ""
            });
            profileGroup.PropertyTypes.Add(new PropertyType(textboxMultiple)
            {
                Alias = "signature",
                Name = "Signature",
                SortOrder = 0,
                Description = ""
            });
            profileGroup.PropertyTypes.Add(new PropertyType(datetime)
            {
                Alias = "lastActiveDate",
                Name = "Last Active Date",
                SortOrder = 0,
                Description = ""
            });
            memType.PropertyGroups.Add(profileGroup);

            // Create the Settings group/tab
            var settingsGroup = new PropertyGroup
            {
                Name = "Settings",
            };
            settingsGroup.PropertyTypes.Add(new PropertyType(truefalse)
            {
                Alias = "canEditOtherMembers",
                Name = "Can Edit Other Members",
                SortOrder = 0,
                Description = "Enable this and the user can edit other members posts, their profiles and ban members (Usually use in conjunction with moderate permissions)."
            });
            settingsGroup.PropertyTypes.Add(new PropertyType(truefalse)
            {
                Alias = "disableEmailNotifications",
                Name = "Disable Email Notifications",
                SortOrder = 0,
                Description = ""
            });
            settingsGroup.PropertyTypes.Add(new PropertyType(truefalse)
            {
                Alias = "disablePosting",
                Name = "Disable Posting",
                SortOrder = 0,
                Description = ""
            });
            settingsGroup.PropertyTypes.Add(new PropertyType(truefalse)
            {
                Alias = "disablePrivateMessages",
                Name = "Disable Private Messages",
                SortOrder = 0,
                Description = ""
            });
            settingsGroup.PropertyTypes.Add(new PropertyType(truefalse)
            {
                Alias = "disableFileUploads",
                Name = "Disable File Uploads",
                SortOrder = 0,
                Description = ""
            });
            settingsGroup.PropertyTypes.Add(new PropertyType(numeric)
            {
                Alias = "postCount",
                Name = "Post Count",
                SortOrder = 0,
                Description = "The users post count. This is kept like this to help reduce SQL queries and improve performance of the forum"
            });
            memType.PropertyGroups.Add(settingsGroup);

            // Create the Settings group/tab
            var socialGroup = new PropertyGroup
            {
                Name = "Social Tokens",
            };
            socialGroup.PropertyTypes.Add(new PropertyType(textstring)
            {
                Name = "Facebook Access Token",
                Alias = "facebookAccessToken",
                SortOrder = 0,
                Description = ""
            });
            socialGroup.PropertyTypes.Add(new PropertyType(textstring)
            {
                Name = "Google Access Token",
                Alias = "googleAccessToken",
                SortOrder = 0,
                Description = ""
            });
            socialGroup.PropertyTypes.Add(new PropertyType(textstring)
            {
                Name = "Microsoft Access Token",
                Alias = "microsoftAccessToken",
                SortOrder = 0,
                Description = ""
            });
            memType.PropertyGroups.Add(socialGroup);

            // Add Slug
            memType.AddPropertyType(new PropertyType(textstring)
            {
                Name = "Slug",
                Alias = "slug",
                SortOrder = 0,
                Description = "This is what we use to look up the member in the front end"
            });

            _memberTypeService.Save(memType);

            return memType;
        }

        #endregion

    }
}