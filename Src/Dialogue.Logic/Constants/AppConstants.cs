using System;

namespace Dialogue.Logic.Constants
{
    public static class AppConstants
    {

        //Activity Keys
        public const string KeyBadgeId = @"BadgeID";
        public const string KeyUserId = @"UserID";
        public const string Equality = @"=";
        public const string Separator = @",";

        // Cache keys
        public const string MessageViewBagName = "ViewBagKeyHere";
        public const string PartialCachePrefix = "tonystark-";
        public const string SiteSettingsKey = "our-sitesettings";

        public const string ViewMacroDirectory = "~/Views/MacroPartials/";
        public const string MemberEmailConfirmationCookieName = "MVCForumEmailConfirmation";

        public const string AssetsImagePath = "~/App_Plugins/Dialogue/Content/Images/";

        // Paths
        public static string MemberUploadPath = String.Concat(UploadFolderPath, "{0}/{1}");

        // Default Image Sizes
        public const int GravatarPostSize = 50;
        public const int GravatarTopicSize = 35;
        public const int GravatarProfileSize = 85;
        public const int GravatarLeaderboardSize = 25;

        /// <summary>
        /// A short cache time to help with speeding up the site
        /// </summary>
        public const int DefaultCacheLengthInSeconds = 600;

        // Doctypes
        public const string DocTypeForumRoot = "Dialogue";
        public const string DocTypeForumCategory = "DialogueCategory";
        public const string DocTypeLogin = "Dialoguelogin";
        public const string DocTypeRegister = "DialogueRegister";
        public const string DocTypeCreateTopic = "DialogueCreateTopic";
        public const string DocTypeEditMember = "DialogueEditMember";
        public const string DocTypeSearchMembers = "DialogueSearchMembers";
        public const string DocTypeSendPrivateMessage = "DialogueSendPrivateMessage";
        
        // Pages and Paging
        public const int ActiveTopicsListSize = 20;
        public const int PagingGroupSize = 10;
        public const int PrivateMessageListSize = 20;
        public const string PagingUrlFormat = "{0}?p={1}";

        //Querystring names
        public const string PostOrderBy = "order";
        public const string AllPosts = "all";

        // Last Activity Time Check
        public const int TimeSpanInMinutesToDoCheck = 10;
        public const int TimeSpanInMinutesToShowMembers = 20;

        //Uploads
        public const string UploadFolderPath = "~/media/MemberUploads/";

        // Page Names
        public const string PageUrlLeaderboard = "leaderboard";
        public const string PageUrlActivity = "activity";
        public const string PageUrlFavourites = "favourite";
        public const string PageUrlPostReport = "postreport";
        public const string PageUrlEditPost = "editpost";
        public const string PageUrlMessageInbox = "messageinbox";
        public const string PageUrlMessageOutbox = "messageoutbox";
        public const string PageUrlCreatePrivateMessage = "createmessage";
        public const string PageUrlViewPrivateMessage = "viewmessage";
        public const string PageUrlViewReportMember = "reportmember";
        public const string PageUrlEditMember = "editmember";
        public const string PageUrlChangePassword = "changepassword";
        public const string PageUrlSearch = "search";
        public const string PageUrlTopicsRss = "topicsrss";
        public const string PageUrlActivityRss = "activityrss";
        public const string PageUrlCategoryRss = "categoryrss";
        public const string PageUrlCreateTopic = "create";
        public const string PageUrlBadges = "badges";
        public const string PageUrlEmailConfirmation = "emailconfirmation";
        public const string PageUrlSpamOverview = "spamoverview";
        public const string PageUrlAuthorise = "authorise";

        // Default Member Group
        public const string MemberGroupDefault = "Dialogue Standard";
        public const string MemberTypeAlias = "DialogueMember";
        public const string MemberTypeName = "Dialogue Member";

        // QueryStrings
        public const string LogOut = "logout";

        // Editor
        //public const string EditorType = "tinymceeditor";
        public const string EditorType = "markdowneditor";

        ///-------------------
        /// Node Property Keys
        ///-------------------
        
        // General
        public const string PropMainHeading = "mainHeading";
        public const string PropBodyText = "bodyText";
        public const string PropViewMacros = "viewMacros";
        public const string PropHideFromSiteMap = "hideFromSitemap";

        // Members
        public const string PropMemberEmail = "email";
        public const string PropMemberSlug = "slug";
        public const string PropMemberCanEditOtherUsers = "canEditOtherMembers";
        public const string PropMemberSignature = "signature";
        public const string PropMemberWebsite = "website";
        public const string PropMemberTwitter = "twitter";
        public const string PropMemberAvatar = "avatar";
        public const string PropMemberUmbracoMemberComments = "umbracoMemberComments";
        public const string PropMemberLastActiveDate = "lastActiveDate";
        public const string PropMemberDisableEmailNotifications = "disableEmailNotifications";
        public const string PropMemberDisablePosting = "disablePosting";
        public const string PropMemberDisablePrivateMessages = "disablePrivateMessages";
        public const string PropMemberDisableFileUploads = "disableFileUploads";
        public const string PropMemberPostCount = "postCount";
        public const string PropMemberFacebookAccessToken = "facebookAccessToken";
        public const string PropMemberFacebookId = "facebookId";
        public const string PropMemberGoogleAccessToken = "googleAccessToken";
        public const string PropMemberGoogleId = "googleId";
        public const string PropMemberUmbracoMemberApproved = "umbracoMemberApproved";
        public const string PropMemberUmbracoMemberLockedOut = "umbracoMemberLockedOut";
        public const string PropMemberUmbracoMemberLastLockoutDate = "umbracoMemberLastLockoutDate";
        public const string PropMemberUmbracoMemberLastLogin = "umbracoMemberLastLogin";
        
        // Seo
        public const string PropPageTitle = "pageTitle";
        public const string PropMetaDesc = "metaDescription";

        // Umbraco Specific Properties
        public const string PropUmbracoNaviHide = "umbracoNaviHide";
        public const string PropShowInFooter = "showInFooter";
        public const string PropConversionCodes = "conversionCodes";

        // Url Name Properties
        public const string PropTopicUrlName = "topicUrlName";
        public const string PropDialogueUrlName = "dialogueUrlName";
        public const string PropMemberUrlName = "memberUrlName";

        // Permiossions
        public const string PermissionAttachFiles = "Attach Files";
        public const string PermissionCreatePolls = "Create Polls";
        public const string PermissionVoteInPolls = "Vote In Polls";
        public const string PermissionCreateTopics = "Create Topics";
        public const string PermissionModerate = "Moderate";
        public const string PermissionDenyAccess = "Deny Access";
        public const string PermissionReadOnly = "Read Only";

        // Main admin role [This should never be changed]
        public const string AdminRoleName = "Dialogue Admin";

        // Main guest role [This should never be changed]
        // This is the role a non logged in user defaults to
        public const string GuestRoleName = "Dialogue Guest";
    }
}