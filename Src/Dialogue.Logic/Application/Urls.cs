using System.Web;
using Dialogue.Logic.Constants;

namespace Dialogue.Logic.Application
{
    public static partial class Urls
    {
        //TODO - Refactor this, must be a better way?
        public enum UrlType
        {
            Topic,
            Member,
            Login,
            Register,
            Dialogue,
            Leaderboard,
            TopicCreate,
            EditMember,
            SearchMembers,
            Search,
            PrivateMessageCreate,
            TopicsRss,
            Badges,
            Activity,
            ActivityRss,
            CategoryRss,
            FacebookLogin,
            GoogleLogin,
            Favourites,
            PostDelete,
            PostReport,
            EditPost,
            FileDelete,
            MessageInbox,
            MessageOutbox,
            MessageCreate,
            MessageView,
            KillSpammer,
            BanMember,
            UnBanMember,
            ReportMember,
            ChangePassword,
            EmailConfirmation,
            SpamOverview,
            Authorise
        }

        public static string UrlTypeName(UrlType e)
        {
            switch (e)
            {
                case UrlType.Topic:
                    return Dialogue.Settings().TopicUrlName;
                case UrlType.Member:
                    return Dialogue.Settings().MemberUrlName;
                case UrlType.Login:
                    return Dialogue.Settings().LoginUrl;
                case UrlType.Register:
                    return Dialogue.Settings().RegisterUrl;
                case UrlType.TopicCreate:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlCreateTopic);
                case UrlType.EmailConfirmation:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlEmailConfirmation);
                case UrlType.Leaderboard:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlLeaderboard);
                case UrlType.Activity:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlActivity);
                case UrlType.TopicsRss:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlTopicsRss);
                case UrlType.ActivityRss:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlActivityRss);
                case UrlType.CategoryRss:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlCategoryRss);
                case UrlType.Badges:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlBadges);
                case UrlType.Favourites:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlFavourites);
                case UrlType.PostReport:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlPostReport);
                case UrlType.EditPost:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlEditPost);
                case UrlType.MessageInbox:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlMessageInbox);
                case UrlType.MessageOutbox:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlMessageOutbox);
                case UrlType.MessageCreate:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlCreatePrivateMessage);
                case UrlType.MessageView:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlViewPrivateMessage);
                case UrlType.ReportMember:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlViewReportMember);
                case UrlType.EditMember:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlEditMember);
                case UrlType.ChangePassword:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlChangePassword);
                case UrlType.Search:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlSearch);
                case UrlType.SpamOverview:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlSpamOverview);
                case UrlType.Authorise:
                    return GenerateUrl(UrlType.Dialogue, AppConstants.PageUrlAuthorise);
                case UrlType.GoogleLogin:
                    return VirtualPathUtility.ToAbsolute("~/umbraco/Surface/GoogleOAuthSurface/GoogleLogin");
                case UrlType.FacebookLogin:
                    return VirtualPathUtility.ToAbsolute("~/umbraco/Surface/FacebookOAuthSurface/FacebookLogin");
                case UrlType.PostDelete:
                    return VirtualPathUtility.ToAbsolute("~/umbraco/Surface/DialoguePostSurface/DeletePost");
                case UrlType.FileDelete:
                    return VirtualPathUtility.ToAbsolute("~/umbraco/Surface/DialogueUploadSurface/DeleteUploadedFile");
                case UrlType.KillSpammer:
                    return VirtualPathUtility.ToAbsolute("~/umbraco/Surface/DialogueMemberSurface/KillSpammer");
                case UrlType.BanMember:
                    return VirtualPathUtility.ToAbsolute("~/umbraco/Surface/DialogueMemberSurface/BanMember");
                case UrlType.UnBanMember:
                    return VirtualPathUtility.ToAbsolute("~/umbraco/Surface/DialogueMemberSurface/UnBanMember");
                default:
                    return Dialogue.Settings().DialogueUrlName;
            }
        }

        public static string GenerateUrl(UrlType e, string slug)
        {
            return VirtualPathUtility.ToAbsolute(string.Format("~{0}{1}/{2}/", Dialogue.Settings().ForumRootUrl, UrlTypeName(e), HttpUtility.HtmlDecode(slug)));            
        }

        public static string GenerateUrl(UrlType e)
        {
            return VirtualPathUtility.ToAbsolute(string.Format("~{0}", UrlTypeName(e)));
        }

        public static string GenerateFileUrl(string filePath)
        {
            return VirtualPathUtility.ToAbsolute(filePath);
        }
    }
}
