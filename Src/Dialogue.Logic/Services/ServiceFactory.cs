using System.Web;

namespace Dialogue.Logic.Services
{
    public static partial class ServiceFactory
    {
        /// <summary>
        /// Get an instance of the Activity Service
        /// </summary>
        public static ActivityService ActivityService
        {
            get
            {
                var key = string.Concat("dialogue-", "ActivityService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new ActivityService());
                }
                return HttpContext.Current.Items[key] as ActivityService;
            }
        }

        /// <summary>
        /// Get an instance of the MemberService 
        /// </summary>
        public static MemberService MemberService
        {
            get
            {
                var key = string.Concat("dialogue-", "MemberService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new MemberService());
                }
                return HttpContext.Current.Items[key] as MemberService;
            }
        }

        /// <summary>
        /// Get an instance of the BadgeService
        /// </summary>
        public static BadgeService BadgeService
        {
            get
            {
                var key = string.Concat("dialogue-", "BadgeService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new BadgeService());
                }
                return HttpContext.Current.Items[key] as BadgeService;
            }
        }

        /// <summary>
        /// Get an instance of the BannedEmailService
        /// </summary>
        public static BannedEmailService BannedEmailService
        {
            get
            {
                var key = string.Concat("dialogue-", "BannedEmailService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new BannedEmailService());
                }
                return HttpContext.Current.Items[key] as BannedEmailService;
            }
        }

        /// <summary>
        /// Get an instance of the BannedWordService
        /// </summary>
        public static BannedWordService BannedWordService
        {
            get
            {
                var key = string.Concat("dialogue-", "BannedWordService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new BannedWordService());
                }
                return HttpContext.Current.Items[key] as BannedWordService;
            }
        }

        /// <summary>
        /// Get an instance of the CategoryNotificationService
        /// </summary>
        public static CategoryNotificationService CategoryNotificationService
        {
            get
            {
                var key = string.Concat("dialogue-", "CategoryNotificationService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new CategoryNotificationService());
                }
                return HttpContext.Current.Items[key] as CategoryNotificationService;
            }
        }

        /// <summary>
        /// Get an instance of the CategoryPermissionService
        /// </summary>
        public static CategoryPermissionService CategoryPermissionService
        {
            get
            {
                var key = string.Concat("dialogue-", "CategoryPermissionService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new CategoryPermissionService());
                }
                return HttpContext.Current.Items[key] as CategoryPermissionService;
            }
        }

        /// <summary>
        /// Get an instance of the CategoryService
        /// </summary>
        public static CategoryService CategoryService
        {
            get
            {
                var key = string.Concat("dialogue-", "CategoryService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new CategoryService());
                }
                return HttpContext.Current.Items[key] as CategoryService;
            }
        }

        /// <summary>
        /// Get an instance of the EmailService
        /// </summary>
        public static EmailService EmailService
        {
            get
            {
                var key = string.Concat("dialogue-", "EmailService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new EmailService());
                }
                return HttpContext.Current.Items[key] as EmailService;
            }
        }

        /// <summary>
        /// Get an instance of the MemberPointsService
        /// </summary>
        public static MemberPointsService MemberPointsService
        {
            get
            {
                var key = string.Concat("dialogue-", "MemberPointsService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new MemberPointsService());
                }
                return HttpContext.Current.Items[key] as MemberPointsService;
            }
        }

        /// <summary>
        /// Get an instance of the PermissionService
        /// </summary>
        public static PermissionService PermissionService
        {
            get
            {
                var key = string.Concat("dialogue-", "PermissionService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new PermissionService());
                }
                return HttpContext.Current.Items[key] as PermissionService;
            }
        }

        /// <summary>
        /// Get an instance of the PollService
        /// </summary>
        public static PollService PollService
        {
            get
            {
                var key = string.Concat("dialogue-", "PollService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new PollService());
                }
                return HttpContext.Current.Items[key] as PollService;
            }
        }

        /// <summary>
        /// Get an instance of the PostService
        /// </summary>
        public static PostService PostService
        {
            get
            {
                var key = string.Concat("dialogue-", "PostService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new PostService());
                }
                return HttpContext.Current.Items[key] as PostService;
            }
        }

        /// <summary>
        /// Get an instance of the PrivateMessageService
        /// </summary>
        public static PrivateMessageService PrivateMessageService
        {
            get
            {
                var key = string.Concat("dialogue-", "PrivateMessageService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new PrivateMessageService());
                }
                return HttpContext.Current.Items[key] as PrivateMessageService;
            }
        }

        /// <summary>
        /// Get an instance of the TopicNotificationService
        /// </summary>
        public static TopicNotificationService TopicNotificationService
        {
            get
            {
                var key = string.Concat("dialogue-", "TopicNotificationService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new TopicNotificationService());
                }
                return HttpContext.Current.Items[key] as TopicNotificationService;
            }
        }

        /// <summary>
        /// Get an instance of the TopicService
        /// </summary>
        public static TopicService TopicService
        {
            get
            {
                var key = string.Concat("dialogue-", "TopicService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new TopicService());
                }
                return HttpContext.Current.Items[key] as TopicService;
            }
        }

        /// <summary>
        /// Get an instance of the UploadedFileService
        /// </summary>
        public static UploadedFileService UploadedFileService
        {
            get
            {
                var key = string.Concat("dialogue-", "UploadedFileService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new UploadedFileService());
                }
                return HttpContext.Current.Items[key] as UploadedFileService;
            }
        }

        /// <summary>
        /// Get an instance of the VoteService
        /// </summary>
        public static VoteService VoteService
        {
            get
            {
                var key = string.Concat("dialogue-", "VoteService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new VoteService());
                }
                return HttpContext.Current.Items[key] as VoteService;
            }
        }

        /// <summary>
        /// Get an instance of the favourite service
        /// </summary>
        public static FavouriteService FavouriteService
        {
            get
            {
                var key = string.Concat("dialogue-", "FavouriteService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new FavouriteService());
                }
                return HttpContext.Current.Items[key] as FavouriteService;
            }
        }

        /// <summary>
        /// Gets and instance of the report service
        /// </summary>
        public static ReportService ReportService
        {
            get
            {
                var key = string.Concat("dialogue-", "ReportService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new ReportService());
                }
                return HttpContext.Current.Items[key] as ReportService;
            }
        }

        /// <summary>
        /// Gets and instance of the BannedLinkService
        /// </summary>
        public static BannedLinkService BannedLinkService
        {
            get
            {
                var key = string.Concat("dialogue-", "BannedLinkService");
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, new BannedLinkService());
                }
                return HttpContext.Current.Items[key] as BannedLinkService;
            }
        }

    }
}