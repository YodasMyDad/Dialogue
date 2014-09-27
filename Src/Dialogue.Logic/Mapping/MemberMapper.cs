using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;
using Umbraco.Core.Models;
using Umbraco.Web;
using Member = Dialogue.Logic.Models.Member;

namespace Dialogue.Logic.Mapping
{
    public static class MemberMapper
    {

        #region Member Mapping

        public static Member MapMember(IMember member, bool populateAll = false)
        {
            return MapMember(member.Id, populateAll);
        }

        public static Member MapMember(IPublishedContent member, bool populateAll = false)
        {
            if (member != null)
            {
                var key = string.Format("umb-member{0}{1}", member.Id, populateAll);
                if (!HttpContext.Current.Items.Contains(key))
                {
                    var siteMember = new Member();

                    // Map Properties
                    MapMemberProperties(member, siteMember);

                    // Get points
                    siteMember.Points = ContextPerRequest.Db.MemberPoints.Where(x => x.MemberId == siteMember.Id).ToList();

                    // Only do extra db calls if populate all is called
                    if (populateAll)
                    {
                        var badgeIds = ContextPerRequest.Db.BadgeToMember.Where(x => x.MemberId == siteMember.Id).Select(x => x.DialogueBadgeId);
                        siteMember.Badges = badgeIds.Any() ? ContextPerRequest.Db.Badge.Where(x => badgeIds.Contains(x.Id)).ToList() : new List<Badge>();
                        siteMember.Votes = ContextPerRequest.Db.Vote.Where(x => x.MemberId == siteMember.Id).ToList();
                    }

                    HttpContext.Current.Items.Add(key, siteMember);
                }
                return HttpContext.Current.Items[key] as Member;
            }
            return null;
        }

        public static Member MapMember(int id, bool populateAll = false)
        {
            var member = AppHelpers.UmbMemberHelper().GetById(id);
            return MapMember(member, populateAll);
        }

        public static List<Member> MapMember(List<int> memberids, bool populateAll = false)
        {
            var key = string.Format("umb-members-{0}", string.Join("-", memberids.Select(x => x.ToString()).ToArray()));
            if (!HttpContext.Current.Items.Contains(key))
            {
                var mappedMembers = new List<IPublishedContent>();
                foreach (var member in memberids.Distinct())
                {
                    mappedMembers.Add(AppHelpers.UmbMemberHelper().GetById(member));
                }
                return MapMember(mappedMembers);
            }
            return HttpContext.Current.Items[key] as List<Member>;
        }

        public static List<Member> MapMember(List<IPublishedContent> members, bool populateAll = false)
        {
            if (members.Any())
            {
                var ids = members.Select(x => x.Id).ToList();
                var key = string.Format("umb-members-{0}", string.Join("-", ids.Select(x => x.ToString()).ToArray()));
                if (!HttpContext.Current.Items.Contains(key))
                {
                    var siteMembers = new List<Member>();
                    var allMembersPoints = ContextPerRequest.Db.MemberPoints.Where(x => ids.Contains(x.MemberId)).ToList();
                    var allBadges = new List<Badge>();
                    var allMembersVotes = new List<Vote>();
                    var allMembersBadges = new List<BadgeToMember>();

                    if (populateAll)
                    {
                        // Only now populate Badges, Votes and Checktimes
                        allMembersBadges = ContextPerRequest.Db.BadgeToMember.Where(x => ids.Contains(x.MemberId)).ToList();
                        allBadges = ContextPerRequest.Db.Badge.ToList();
                        allMembersVotes = ContextPerRequest.Db.Vote.Where(x => ids.Contains(x.MemberId)).ToList();
                    }

                    foreach (var member in members)
                    {
                        // Create new member
                        var siteMember = new Member();

                        // Map Properties
                        MapMemberProperties(member, siteMember);

                        // Member Points
                        siteMember.Points = allMembersPoints.Where(x => x.MemberId == siteMember.Id).ToList();

                        if (populateAll)
                        {
                            // Only now populate Badges, Votes and Checktimes                        
                            var badgeIds = allMembersBadges.Where(x => x.MemberId == siteMember.Id).Select(x => x.DialogueBadgeId);
                            siteMember.Badges = allBadges.Where(x => badgeIds.Contains(x.Id)).ToList();
                            siteMember.Votes = allMembersVotes.Where(x => x.MemberId == siteMember.Id).ToList();
                        }

                        siteMembers.Add(siteMember);
                    }

                    HttpContext.Current.Items.Add(key, siteMembers);
                }
                return HttpContext.Current.Items[key] as List<Member>;
            }
            return new List<Member>();
        }

        public static MembersPaged MapPagedMember(List<Member> members, int pageIndex, int pageSize, int totalRecords)
        {
            return new MembersPaged
                {
                    Members = members,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                };
        }

        private static void MapMemberProperties(IPublishedContent member, Member siteMember)
        {

            siteMember.Id = member.Id;
            siteMember.UserName = member.Name;
            siteMember.DateCreated = member.CreateDate;

            siteMember.Email = member.GetPropertyValue<string>(AppConstants.PropMemberEmail);
            siteMember.Signature = member.GetPropertyValue<string>(AppConstants.PropMemberSignature);
            siteMember.Website = member.GetPropertyValue<string>(AppConstants.PropMemberWebsite);
            siteMember.Twitter = member.GetPropertyValue<string>(AppConstants.PropMemberTwitter);
            siteMember.Avatar = member.GetPropertyValue<string>(AppConstants.PropMemberAvatar);
            siteMember.Comments = member.GetPropertyValue<string>(AppConstants.PropMemberUmbracoMemberComments);
            siteMember.LastActiveDate = member.GetPropertyValue<DateTime>(AppConstants.PropMemberLastActiveDate);
            siteMember.Slug = member.GetPropertyValue<string>(AppConstants.PropMemberSlug);
            siteMember.CanEditOtherMembers = member.GetPropertyValue<bool>(AppConstants.PropMemberCanEditOtherUsers);

            siteMember.DisableEmailNotifications = member.GetPropertyValue<bool>(AppConstants.PropMemberDisableEmailNotifications);
            siteMember.DisablePosting = member.GetPropertyValue<bool>(AppConstants.PropMemberDisablePosting);
            siteMember.DisablePrivateMessages = member.GetPropertyValue<bool>(AppConstants.PropMemberDisablePrivateMessages);
            siteMember.DisableFileUploads = member.GetPropertyValue<bool>(AppConstants.PropMemberDisableFileUploads);
            siteMember.PostCount = member.GetPropertyValue<int>(AppConstants.PropMemberPostCount);

            siteMember.FacebookAccessToken = member.GetPropertyValue<string>(AppConstants.PropMemberFacebookAccessToken);
            siteMember.FacebookId = member.GetPropertyValue<string>(AppConstants.PropMemberFacebookId);
            siteMember.GoogleAccessToken = member.GetPropertyValue<string>(AppConstants.PropMemberGoogleAccessToken);
            siteMember.GoogleId = member.GetPropertyValue<string>(AppConstants.PropMemberGoogleId);

            siteMember.IsApproved = member.GetPropertyValue<bool>(AppConstants.PropMemberUmbracoMemberApproved);
            siteMember.IsLockedOut = member.GetPropertyValue<bool>(AppConstants.PropMemberUmbracoMemberLockedOut);
            siteMember.LastLoginDate = member.GetPropertyValue<DateTime>(AppConstants.PropMemberUmbracoMemberLastLogin);

            var roleNames = AppHelpers.UmbServices().MemberService.GetAllRoles(siteMember.Id).ToList();
            // We do this so it's only one trip, and then cache per request
            var allGroups = AppHelpers.GetAllMemberGroups();
            var groups = new List<IMemberGroup>();

            // Fish out the roles/groups we need
            foreach (var role in roleNames)
            {
                groups.Add(allGroups.FirstOrDefault(x => x.Name == role));
            }
            siteMember.Groups = groups;
        }

        #endregion
    }
}