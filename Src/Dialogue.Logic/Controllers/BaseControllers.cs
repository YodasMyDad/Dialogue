using System;
using System.Text;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Models;
using Dialogue.Logic.Services;
using Umbraco.Web.Mvc;

namespace Dialogue.Logic.Controllers
{

    // TODO - This is duplicated code! Refactor

    #region Render Controller
    public partial class BaseRenderController : RenderMvcController
    {
        protected readonly UnitOfWorkManager UnitOfWorkManager;

        public BaseRenderController()
        {
            UnitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
        }
        public void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            // We have to put it on two because some umbraco redirects only work with ViewData!!
            ViewData[AppConstants.MessageViewBagName] = messageViewModel;
            TempData[AppConstants.MessageViewBagName] = messageViewModel;
        }
        internal string Lang(string key)
        {
            return AppHelpers.Lang(key);
        }
        internal void LogWarning(string message)
        {
            AppHelpers.LogError(message);
        }
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
        internal void LogError(string message, Exception ex)
        {
            AppHelpers.LogError(message, ex);
        }
        internal void LogError(Exception ex)
        {
            AppHelpers.LogError("Dialogue Package Exception", ex);
        }
        internal DialogueSettings Settings
        {
            get
            {
                return Dialogue.Settings();
            }
        }
        internal bool UserIsAuthenticated
        {
            get
            {
                return System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }
        protected string Username
        {
            get
            {
                return UserIsAuthenticated ? System.Web.HttpContext.Current.User.Identity.Name : null;
            }
        }

        internal Member CurrentMember
        {
            get
            {
                if (UserIsAuthenticated)
                {
                    return ServiceFactory.MemberService.CurrentMember();
                }
                return null;
            }
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
    } 
    #endregion

    #region Surface Controller
    public partial class BaseSurfaceController : SurfaceController
    {
        protected readonly UnitOfWorkManager UnitOfWorkManager;

        public BaseSurfaceController()
        {
            UnitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
        }

        public void ShowModelErrors()
        {
            var message = new GenericMessageViewModel();
            var sb = new StringBuilder();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    sb.AppendFormat("<li>{0}</li>", error.ErrorMessage);
                }
            }
            var fullMessage = sb.ToString();
            if (!string.IsNullOrEmpty(fullMessage))
            {
                message.Message = string.Concat("<ul>", fullMessage, "</ul>");
                message.MessageType = GenericMessages.Danger;
                ShowMessage(message);
            }
        }
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

        public void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            // We have to put it on two because some umbraco redirects only work with ViewData!!
            ViewData[AppConstants.MessageViewBagName] = messageViewModel;
            TempData[AppConstants.MessageViewBagName] = messageViewModel;
        }
        internal string Lang(string key)
        {
            return AppHelpers.Lang(key);
        }
        internal void LogWarning(string message)
        {
            AppHelpers.LogError(message);
        }
        internal bool UserIsAuthenticated
        {
            get
            {
                return System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }
        protected string Username
        {
            get
            {
                return UserIsAuthenticated ? System.Web.HttpContext.Current.User.Identity.Name : null;
            }
        }
        internal void LogError(string message, Exception ex)
        {
            AppHelpers.LogError(message, ex);
        }
        internal void LogError(Exception ex)
        {
            AppHelpers.LogError("Dialogue Package Exception", ex);
        }
        internal DialogueSettings Settings
        {
            get
            {
                return Dialogue.Settings();
            }
        }
        internal Member CurrentMember
        {
            get
            {
                if (UserIsAuthenticated)
                {
                    return ServiceFactory.MemberService.CurrentMember();
                }
                return null;
            }
        }
    } 
    #endregion

}