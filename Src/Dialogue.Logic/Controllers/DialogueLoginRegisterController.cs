using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Member = Dialogue.Logic.Models.Member;

namespace Dialogue.Logic.Controllers
{
    #region Render Controllers
    public partial class DialogueLoginController : BaseController
    {
        protected readonly BannedEmailService BannedEmailService;
        public DialogueLoginController()
        {
            BannedEmailService = new BannedEmailService();
        }

        [ModelStateToTempData]
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

    }

    public partial class DialogueRegisterController : BaseController
    {
        protected readonly BannedEmailService BannedEmailService;
        public DialogueRegisterController()
        {
            BannedEmailService = new BannedEmailService();
        }


        [ModelStateToTempData]
        public override ActionResult Index(RenderModel model)
        {
            // Create the empty view model
            var pageModel = new Models.RegisterModel(model.Content);

            DialogueMapper.PopulateCommonUmbracoProperties(pageModel, model.Content);

            // Return the model to the current template
            return View(PathHelper.GetThemeViewPath("Register"), pageModel);
        }

    }

    #endregion

    #region SurefaceController

    public class DialogueLoginRegisterSurfaceController : BaseSurfaceController
    {
        protected readonly BannedEmailService BannedEmailService;
        protected readonly BannedWordService BannedWordService;
        protected readonly EmailService EmailService;
        protected readonly ActivityService ActivityService;
        public DialogueLoginRegisterSurfaceController()
        {
            BannedEmailService = new BannedEmailService();
            BannedWordService = new BannedWordService();
            EmailService = new EmailService();
            ActivityService = new ActivityService();
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
        public ActionResult RegisterForm()
        {
            var viewModel = new RegisterViewModel();
            
            // See if a return url is present or not and add it
            var returnUrl = Request["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
            {
                viewModel.ReturnUrl = returnUrl;
            }

            return View(PathHelper.GetThemePartialViewPath("RegisterForm"), viewModel);
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
        [ModelStateToTempData]
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
            
            // Hack to show form validation
            ShowModelErrors();
            
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ModelStateToTempData]
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
                LogError(string.Format("Error resetting password for {0}", model.EmailAddress), ex);
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("<p>{0}</p>", string.Format(Lang("Members.ForgotPassword.Email"), Settings.ForumName));
                sb.AppendFormat("<p><b>{0}</b></p>", newPassword);
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
            // Hack to show form validation
            ShowModelErrors();
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ModelStateToTempData]
        public ActionResult Register(RegisterViewModel userModel)
        {
            var forumReturnUrl = Settings.ForumRootUrl;
            var newMemberGroup = Settings.Group;
            if (userModel.ForumId != null && userModel.ForumId != Settings.ForumId)
            {
                var correctForum = Dialogue.Settings((int)userModel.ForumId);
                forumReturnUrl = correctForum.ForumRootUrl;
                newMemberGroup = correctForum.Group;
            }

            if (Settings.SuspendRegistration != true)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // First see if there is a spam question and if so, the answer matches
                    if (!string.IsNullOrEmpty(Settings.SpamQuestion))
                    {
                        // There is a spam question, if answer is wrong return with error
                        if (userModel.SpamAnswer == null || userModel.SpamAnswer.ToLower().Trim() != Settings.SpamAnswer.ToLower())
                        {
                            // POTENTIAL SPAMMER!
                            ModelState.AddModelError(string.Empty, Lang("Error.WrongAnswerRegistration"));
                            //ShowModelErrors();
                            return CurrentUmbracoPage();
                        }
                    }

                    // Secondly see if the email is banned
                    if (BannedEmailService.EmailIsBanned(userModel.Email))
                    {
                        ModelState.AddModelError(string.Empty, Lang("Error.EmailIsBanned"));
                        //ShowModelErrors();
                        return CurrentUmbracoPage();
                    }

                    var userToSave = AppHelpers.UmbMemberHelper().CreateRegistrationModel(AppConstants.MemberTypeAlias);
                    userToSave.Username = BannedWordService.SanitiseBannedWords(userModel.UserName);
                    userToSave.Name = userToSave.Username;
                    userToSave.UsernameIsEmail = false;
                    userToSave.Email = userModel.Email;
                    userToSave.Password = userModel.Password;

                    var homeRedirect = false;

                    //var createStatus = MembershipService.CreateUser(userToSave);
                    MembershipCreateStatus createStatus;
                    AppHelpers.UmbMemberHelper().RegisterMember(userToSave, out createStatus, false);

                    if (createStatus != MembershipCreateStatus.Success)
                    {
                        ModelState.AddModelError(string.Empty, MemberService.ErrorCodeToString(createStatus));
                    }
                    else
                    {
                        // Get the umbraco member
                        var umbracoMember = AppHelpers.UmbServices().MemberService.GetByUsername(userToSave.Username);

                        // Set the role/group they should be in
                        AppHelpers.UmbServices().MemberService.AssignRole(umbracoMember.Id, newMemberGroup.Name);

                        // Now check settings, see if users need to be manually authorised
                        // OR Does the user need to confirm their email
                        var manuallyAuthoriseMembers = Settings.ManuallyAuthoriseNewMembers;
                        var memberEmailAuthorisationNeeded = Settings.NewMembersMustConfirmAccountsViaEmail;
                        if (manuallyAuthoriseMembers || memberEmailAuthorisationNeeded)
                        {
                            umbracoMember.IsApproved = false;
                        }

                        // Do a save on the member
                        AppHelpers.UmbServices().MemberService.Save(umbracoMember);

                        if (Settings.EmailAdminOnNewMemberSignup)
                        {
                            var sb = new StringBuilder();
                            sb.AppendFormat("<p>{0}</p>", string.Format(Lang("Members.NewMemberRegistered"), Settings.ForumName, Settings.ForumRootUrl));
                            sb.AppendFormat("<p>{0} - {1}</p>", userToSave.Username, userToSave.Email);
                            var email = new Email
                            {
                                EmailTo = Settings.AdminEmailAddress,
                                EmailFrom = Settings.NotificationReplyEmailAddress,
                                NameTo = Lang("Members.Admin"),
                                Subject = Lang("Members.NewMemberSubject")
                            };
                            email.Body = EmailService.EmailTemplate(email.NameTo, sb.ToString());
                            EmailService.SendMail(email);
                        }

                        // Fire the activity Service
                        ActivityService.MemberJoined(MemberMapper.MapMember(umbracoMember));

                        var userMessage = new GenericMessageViewModel();

                        // Set the view bag message here
                        if (manuallyAuthoriseMembers)
                        {
                            userMessage.Message = Lang("Members.NowRegisteredNeedApproval");
                            userMessage.MessageType = GenericMessages.Success;
                        }
                        else if (memberEmailAuthorisationNeeded)
                        {
                            userMessage.Message = Lang("Members.MemberEmailAuthorisationNeeded");
                            userMessage.MessageType = GenericMessages.Success;
                        }
                        else
                        {
                            // If not manually authorise then log the user in
                            FormsAuthentication.SetAuthCookie(userToSave.Username, false);
                            userMessage.Message = Lang("Members.NowRegistered");
                            userMessage.MessageType = GenericMessages.Success;
                        }

                        //Show the message
                        ShowMessage(userMessage);

                        if (!manuallyAuthoriseMembers && !memberEmailAuthorisationNeeded)
                        {
                            homeRedirect = true;
                        }

                        try
                        {
                            unitOfWork.Commit();

                            // Only send the email if the admin is not manually authorising emails or it's pointless
                            SendEmailConfirmationEmail(umbracoMember);

                            if (homeRedirect)
                            {
                                if (Url.IsLocalUrl(userModel.ReturnUrl) && userModel.ReturnUrl.Length > 1 && userModel.ReturnUrl.StartsWith("/")
                                && !userModel.ReturnUrl.StartsWith("//") && !userModel.ReturnUrl.StartsWith("/\\"))
                                {
                                    return Redirect(userModel.ReturnUrl);
                                }
                                return Redirect(forumReturnUrl);
                            }
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LogError("Eror during member registering", ex);
                            FormsAuthentication.SignOut();
                            ModelState.AddModelError(string.Empty, Lang("Errors.GenericMessage"));
                        }
                    }
                    //ShowModelErrors();
                    return CurrentUmbracoPage();
                }
            }


            return Redirect(forumReturnUrl);
        }

        private void SendEmailConfirmationEmail(IMember userToSave)
        {
            var manuallyAuthoriseMembers = Settings.ManuallyAuthoriseNewMembers;
            var memberEmailAuthorisationNeeded = Settings.ManuallyAuthoriseNewMembers;
            if (manuallyAuthoriseMembers == false && memberEmailAuthorisationNeeded)
            {
                if (!string.IsNullOrEmpty(userToSave.Email))
                {
                    // SEND AUTHORISATION EMAIL
                    var sb = new StringBuilder();
                    var confirmationLink = string.Concat(AppHelpers.ReturnCurrentDomain(), Url.Action("EmailConfirmation", new { id = userToSave.Id }));
                    sb.AppendFormat("<p>{0}</p>", string.Format(Lang("Members.MemberEmailAuthorisation.EmailBody"),
                                                Settings.ForumName,
                                                string.Format("<p><a href=\"{0}\">{0}</a></p>", confirmationLink)));
                    var email = new Email
                    {
                        EmailFrom = Settings.NotificationReplyEmailAddress,
                        EmailTo = userToSave.Email,
                        NameTo = userToSave.Username,
                        Subject = Lang("Members.MemberEmailAuthorisation.Subject")
                    };
                    email.Body = EmailService.EmailTemplate(email.NameTo, sb.ToString());
                    EmailService.SendMail(email);

                    // ADD COOKIE
                    // We add a cookie for 7 days, which will display the resend email confirmation button
                    // This cookie is removed when they click the confirmation link
                    var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                    {
                        Value = string.Format("{0}#{1}", userToSave.Email, userToSave.Username),
                        Expires = DateTime.Now.AddDays(7)
                    };
                    // Add the cookie.
                    Response.Cookies.Add(myCookie);
                }
            }
        }

        public ActionResult ResendEmailConfirmation(string username)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var user = AppHelpers.UmbServices().MemberService.GetByUsername(username);
                if (user != null)
                {
                    SendEmailConfirmationEmail(user);
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = Lang("Members.MemberEmailAuthorisationNeeded"),
                        MessageType = GenericMessages.Success
                    });
                }
            }
            return RedirectToUmbracoPage(Settings.ForumId);
        }

        /// <summary>
        /// Email confirmation page from the link in the users email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EmailConfirmation(int id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Checkconfirmation
                var user = MemberService.Get(id);
                if (user != null)
                {
                    // Set the user to active
                    user.IsApproved = true;

                    // Delete Cookie and log them in if this cookie is present
                    if (Request.Cookies[AppConstants.MemberEmailConfirmationCookieName] != null)
                    {
                        var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                        {
                            Expires = DateTime.Now.AddDays(-1)
                        };
                        Response.Cookies.Add(myCookie);

                        // Login code
                        FormsAuthentication.SetAuthCookie(user.UserName, false);
                    }

                    // Show a new message
                    // We use temp data because we are doing a redirect
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = Lang("Members.NowApproved"),
                        MessageType = GenericMessages.Success
                    });
                }

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LogError(ex);
                }
            }

            return RedirectToUmbracoPage(Settings.ForumId);
        }
    }

    #endregion
}