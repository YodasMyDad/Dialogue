using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public partial class DialogueSettings
    {
        // Content
        public int ForumId { get; set; }
        public string ForumRootUrl { get; set; }
        public string ForumRootUrlWithDomain { get; set; }
        public string ForumName { get; set; }
        public string ForumDescription { get; set; }

        // Page Urls
        public string LoginUrl { get; set; }
        public string RegisterUrl { get; set; }

        // Urls
        public string TopicUrlName { get; set; }
        public string MemberUrlName { get; set; }
        public string DialogueUrlName { get; set; }     

        // General
        public int FileUploadMaximumFilesize { get; set; }
        public List<string> FileUploadAllowedExtensions { get; set; }
        public bool AllowRssFeeds { get; set; }
        public bool SuspendRegistration { get; set; }
        public bool EnableSpamReporting { get; set; }
        public bool EnableMemberReporting { get; set; }
        public bool AllowEmailSubscriptions { get; set; }
        public bool ManuallyAuthoriseNewMembers { get; set; }
        public bool EmailAdminOnNewMemberSignup { get; set; }
        public bool NewMembersMustConfirmAccountsViaEmail { get; set; }
        public bool AllowMemberSignatures { get; set; }
        public int TopicsPerPage { get; set; }
        public bool AllowPostsToBeMarkedAsSolution { get; set; }
        public int PostsPerPage { get; set; }
        public int ActivitiesPerPage { get; set; }
        public bool AllowPrivateMessages { get; set; }
        public int PrivateMessageInboxSize { get; set; }
        public int PrivateMessageFloodControl { get; set; }
        
        // Points
        public bool AllowPoints { get; set; }
        public int AmountOfPointsBeforeAUserCanVote { get; set; }
        public int PointsAddedPerNewPost { get; set; }
        public int PointsAddedForPositiveVote { get; set; }
        public int PointsDeductedForNegativeVote { get; set; }
        public int PointsAddedForASolution { get; set; }

        // Email
        public string AdminEmailAddress { get; set; }
        public string NotificationReplyEmailAddress { get; set; }

        // Theme
        public string Theme { get; set; }

        // Member Group
        public IMemberGroup Group { get; set; }

        // Spam
        public bool EnableAkismetSpamControl { get; set; }
        public string AkismetKey { get; set; }
        public string SpamQuestion { get; set; }
        public string SpamAnswer { get; set; }

        // Social
        public string FacebookAppId { get; set; }
        public string FacebookAppSecret { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }

        // Meta
        public string PageTitle { get; set; }
        public string MetaDescription { get; set; }

        // Banned Words
        public List<string> BannedWords { get; set; }

        // Banned Emails
        public List<string> BannedEmails { get; set; }

        // Banned Links
        public List<string> BannedLinks { get; set; }

        // Umbraco Properties
        public DateTime LastModified { get; set; }
    }
}