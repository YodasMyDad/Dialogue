namespace Dialogue.Logic.Controllers
{
    using Umbraco.Core.Logging;
    using Umbraco.Web.Models;
    using System;
    using System.Web.Mvc;
    using Application;
    using Constants;
    using Data.Context;
    using Data.UnitOfWork;
    using Models;
    using Services;
    using Umbraco.Web.Mvc;

    public abstract class DialogueBaseController : SurfaceController, IRenderMvcController
    {
        protected readonly UnitOfWorkManager UnitOfWorkManager;

        protected DialogueBaseController()
        {
            UnitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);

            // We do this because we seem to lose the correct culture on some Ajax and VirtualNode requests
            ContextHelper.EnsureCorrectCulture();
        }

        #region Services

        public readonly ActivityService ActivityService = ServiceResolver.Current.Instance<ActivityService>();
        public readonly BadgeService BadgeService = ServiceResolver.Current.Instance<BadgeService>();
        public readonly BannedEmailService BannedEmailService = ServiceResolver.Current.Instance<BannedEmailService>();
        public readonly BannedLinkService BannedLinkService = ServiceResolver.Current.Instance<BannedLinkService>();
        public readonly BannedWordService BannedWordService = ServiceResolver.Current.Instance<BannedWordService>();
        public readonly CategoryNotificationService CategoryNotificationService = ServiceResolver.Current.Instance<CategoryNotificationService>();
        public readonly CategoryPermissionService CategoryPermissionService = ServiceResolver.Current.Instance<CategoryPermissionService>();
        public readonly CategoryService CategoryService = ServiceResolver.Current.Instance<CategoryService>();
        public readonly EmailService EmailService = ServiceResolver.Current.Instance<EmailService>();
        public readonly FavouriteService FavouriteService = ServiceResolver.Current.Instance<FavouriteService>();
        public readonly MemberPointsService MemberPointsService = ServiceResolver.Current.Instance<MemberPointsService>();
        public readonly MemberService MemberService = ServiceResolver.Current.Instance<MemberService>();
        public readonly PermissionService PermissionService = ServiceResolver.Current.Instance<PermissionService>();
        public readonly PollService PollService = ServiceResolver.Current.Instance<PollService>();
        public readonly PostService PostService = ServiceResolver.Current.Instance<PostService>();
        public readonly PrivateMessageService PrivateMessageService = ServiceResolver.Current.Instance<PrivateMessageService>();
        public readonly ReportService ReportService = ServiceResolver.Current.Instance<ReportService>();
        public readonly TopicNotificationService TopicNotificationService = ServiceResolver.Current.Instance<TopicNotificationService>();
        public readonly TopicService TopicService = ServiceResolver.Current.Instance<TopicService>();
        public readonly UploadedFileService UploadedFileService = ServiceResolver.Current.Instance<UploadedFileService>();
        public readonly VoteService VoteService = ServiceResolver.Current.Instance<VoteService>();

        #endregion

        #region Messages

        public void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            // We have to put it on two because some umbraco redirects only work with ViewData!!
            ViewData[AppConstants.MessageViewBagName] = messageViewModel;
            TempData[AppConstants.MessageViewBagName] = messageViewModel;
        }

        #endregion

        #region Logging

        internal void LogWarning(string message)
        {
            AppHelpers.LogError(message);
        }
        internal void LogError(string message, Exception ex)
        {
            AppHelpers.LogError(message, ex);
        }
        internal void LogError(Exception ex)
        {
            AppHelpers.LogError("Dialogue Package Exception", ex);
        }

        #endregion

        #region Redirects
        public ActionResult NoPermission(Topic topic)
        {
            // Trying to be a sneaky mo fo, so tell them
            var message = new GenericMessageViewModel
            {
                Message = Lang("Errors.NoPermission"),
                MessageType = GenericMessages.Warning
            };
            ShowMessage(message);
            return Redirect(topic.Url);
        }


        internal ActionResult ErrorToHomePage(string errorMessage)
        {
            // Use temp data as its a redirect
            ShowMessage(new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.Danger
            });
            // Not allowed in here so
            return new RedirectToUmbracoPageResult(Dialogue.Settings().ForumId);
        }

        internal ActionResult MessageToHomePage(string errorMessage)
        {
            // Use temp data as its a redirect
            ShowMessage(new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.Info
            });
            // Not allowed in here so
            return new RedirectToUmbracoPageResult(Dialogue.Settings().ForumId);
        }
        #endregion

        #region Language Utils
        internal string Lang(string key)
        {
            return AppHelpers.Lang(key);
        }

        #endregion

        #region Membership
        internal bool UserIsAuthenticated => System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

        protected string Username => UserIsAuthenticated ? System.Web.HttpContext.Current.User.Identity.Name : null;

        internal Member CurrentMember
        {
            get
            {
                if (UserIsAuthenticated)
                {
                    return MemberService.CurrentMember();
                }
                return null;
            }
        }
        #endregion

        #region Settings

        internal DialogueSettings Settings => Dialogue.Settings();

        #endregion

        #region Render MVC

        /// <summary>
        /// Checks to make sure the physical view file exists on disk.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        protected bool EnsurePhsyicalViewExists(string template)
        {
            var result = ViewEngines.Engines.FindView(ControllerContext, template, null);
            if (result.View == null)
            {
                LogHelper.Warn<DialogueBaseController>("No physical template file was found for template " + template);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns an ActionResult based on the template name found in the route values and the given model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the template found in the route values doesn't physically exist, then an empty ContentResult will be returned.
        /// </remarks>
        protected ActionResult CurrentTemplate<T>(T model)
        {
            var template = ControllerContext.RouteData.Values["action"].ToString();
            if (!EnsurePhsyicalViewExists(template))
            {
                return HttpNotFound();
            }
            return View(template, model);
        }

        /// <summary>
        /// The default action to render the front-end view.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult Index(RenderModel model)
        {
            return CurrentTemplate(model);
        }

        #endregion
    }


}