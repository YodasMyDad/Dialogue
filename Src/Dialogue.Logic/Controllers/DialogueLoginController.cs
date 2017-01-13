namespace Dialogue.Logic.Controllers
{
    using System;
    using System.Text;
    using System.Web.Mvc;
    using Application;
    using Mapping;
    using Models;
    using Models.ViewModels;
    using Umbraco.Web.Models;
    using Member = Models.Member;

    public partial class DialogueLoginController : DialogueBaseController
    {
        public override ActionResult Index(RenderModel model)
        {
            // Create the empty view model
            var pageModel = new LogOnModel(model.Content);

            DialogueMapper.PopulateCommonUmbracoProperties(pageModel, model.Content);

            // See if a return url is present or not and add it
            var forgotPassword = Request["forgot"];
            if (!string.IsNullOrEmpty(forgotPassword))
            {
                pageModel.ShowForgotPassword = true;
            }

            // Return the model to the current template
            return View(PathHelper.GetThemeViewPath("Login"), pageModel);
        }

        [ChildActionOnly]
        public ActionResult LogOnForm()
        {
            var viewModel = new LogOnViewModel();
            // See if a return url is present or not and add it
            var returnUrl = Request["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
            {
                viewModel.ReturnUrl = returnUrl;
            }
            return View(PathHelper.GetThemePartialViewPath("LogOnForm"), viewModel);
        }



        [ChildActionOnly]
        public ActionResult ForgotPasswordForm()
        {
            var viewModel = new ForgotPasswordViewModel();
            return View(PathHelper.GetThemePartialViewPath("ForgotPasswordForm"), viewModel);
        }

        /// <summary>
        /// Log on post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(LogOnViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var message = new GenericMessageViewModel();
                    var user = new Member();
                    if (MemberService.Login(model.UserName, model.Password))
                    {
                        // Set last login date
                        user = MemberService.Get(model.UserName);
                        if (user.IsApproved && !user.IsLockedOut)
                        {
                            if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 && model.ReturnUrl.StartsWith("/")
                                && !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
                            {
                                return Redirect(model.ReturnUrl);
                            }

                            message.Message = Lang("Members.NowLoggedIn");
                            message.MessageType = GenericMessages.Success;

                            return RedirectToUmbracoPage(Dialogue.Settings().ForumId);
                        }
                    }

                    // Only show if we have something to actually show to the user
                    if (!string.IsNullOrEmpty(message.Message))
                    {
                        ShowMessage(message);
                    }
                    else
                    {
                        if (user.IsApproved)
                        {
                            ModelState.AddModelError(string.Empty, Lang("Members.Errors.NotApproved"));
                        }
                        else if (user.IsLockedOut)
                        {
                            ModelState.AddModelError(string.Empty, Lang("Members.Errors.LockedOut"));
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, Lang("Members.Errors.LogonGeneric"));
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Lang("Members.Errors.LogonGeneric"));
                }
            }
            catch (Exception ex)
            {
                LogError("Error when user logging in", ex);
            }

            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var changePasswordSucceeded = true;
            var currentUser = new Member();
            var newPassword = AppHelpers.RandomString(8);

            try
            {
                if (ModelState.IsValid)
                {
                    currentUser = MemberService.GetByEmail(model.EmailAddress);
                    if (currentUser != null)
                    {
                        changePasswordSucceeded = MemberService.ResetPassword(currentUser, newPassword);
                    }
                    else
                    {
                        changePasswordSucceeded = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error resetting password for {model.EmailAddress}", ex);
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                var sb = new StringBuilder();
                sb.Append($"<p>{string.Format(Lang("Members.ForgotPassword.Email"), Settings.ForumName)}</p>");
                sb.Append($"<p><b>{newPassword}</b></p>");
                var email = new Email
                {
                    EmailFrom = Settings.NotificationReplyEmailAddress,
                    EmailTo = currentUser.Email,
                    NameTo = currentUser.UserName,
                    Subject = Lang("Members.ForgotPassword.Subject")
                };
                email.Body = EmailService.EmailTemplate(email.NameTo, sb.ToString());
                EmailService.SendMail(email);

                // We use temp data because we are doing a redirect
                ShowMessage(new GenericMessageViewModel
                {
                    Message = Lang("Members.ForgotPassword.SuccessMessage"),
                    MessageType = GenericMessages.Success
                });
                return CurrentUmbracoPage();
            }

            ModelState.AddModelError("", Lang("Members.ForgotPassword.ErrorMessage"));
            return CurrentUmbracoPage();
        }

    }


}