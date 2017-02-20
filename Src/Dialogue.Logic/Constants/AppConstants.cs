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
        public static string MemberUploadPath = string.Concat(UploadFolderPath, "{0}/{1}");

        /// <summary>
        /// A short cache time to help with speeding up the site
        /// </summary>
        public const int DefaultCacheLengthInSeconds = 600;
        
        // Pages and Paging
        public const string PagingUrlFormat = "{0}?p={1}";

        //Querystring names
        public const string PostOrderBy = "order";
        public const string AllPosts = "all";

        public const string EditorType = "forumeditor";

        //Uploads
        public const string UploadFolderPath = "~/media/MemberUploads/";

        // QueryStrings
        public const string LogOut = "logout";

        // Editor

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
        public const string PropMemberGoogleAccessToken = "googleAccessToken";
        public const string PropMemberMicrosoftAccessToken = "microsoftAccessToken";
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