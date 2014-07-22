using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Signature { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public string Avatar { get; set; }
        public string Comments { get; set; }
        public DateTime LastActiveDate { get; set; }
        public DateTime DateCreated { get; set; }
        public bool DisableEmailNotifications { get; set; }
        public bool DisablePosting { get; set; }
        public bool DisablePrivateMessages { get; set; }
        public bool DisableFileUploads { get; set; }
        //public IMember BaseMember { get; set; }
        public List<IMemberGroup> Groups { get; set; }
        public string FacebookAccessToken { get; set; }
        public string FacebookId { get; set; }
        public string GoogleAccessToken { get; set; }
        public string GoogleId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime LastLoginDate { get; set; }
        public IList<MemberPoints> Points { get; set; }
        public int TotalPoints
        {
            get
            {
                return Points != null ? Points.Select(x => x.Points).Sum() : 0;
            }
        }

        // Populated only when full populate marked as true
        public IList<Vote> Votes { get; set; }
        public IList<Badge> Badges { get; set; }
        public IList<BadgeTypeTimeLastChecked> BadgeTypesTimeLastChecked { get; set; }
        // Populated only when full populate marked as true

        public string NiceUrl
        {
            get { return UrlTypes.GenerateUrl(UrlTypes.UrlType.Member, AppHelpers.CreateUrl(UserName, "-")); }
        }

        public string MemberImage(int size)
        {
            return AppHelpers.MemberImage(Avatar, Email, Id, size);
        }
    }
}