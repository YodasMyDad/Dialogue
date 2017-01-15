namespace Dialogue.Logic.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Web.Mvc;
    using System.Web.Security;
    using Application;
    using Constants;
    using Models;
    using Models.ViewModels;
    using Routes;
    using Umbraco.Core.Models;
    using Umbraco.Web.Models;

    public partial class DialogueMemberController : DialogueBaseController
    {
        private readonly IMemberGroup _membersGroup;

        public DialogueMemberController()
        {
            _membersGroup = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        /// <summary>
        /// Used to render the Members profile (virtual node)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="membername">
        /// The slug which we use to look up the member
        /// </param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ActionResult Show(RenderModel model, string membername, int? p = null)
        {
            var memPage = model.Content as DialogueVirtualPage;
            if (memPage == null)
            {
                throw new InvalidOperationException("The RenderModel.Content instance must be of type " + typeof(DialogueVirtualPage));
            }

            if (string.IsNullOrEmpty(membername))
            {
                return ErrorToHomePage(Lang("Errors.GenericMessage"));
            }

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var member = MemberService.GetUserBySlug(membername, true);
                var loggedonId = UserIsAuthenticated ? CurrentMember.Id : 0;
                var viewModel = new ViewMemberViewModel(model.Content)
                {
                    User = member,
                    LoggedOnUserId = loggedonId,
                    PageTitle = string.Concat(member.UserName, Lang("Members.ProfileTitle")),
                    PostCount = member.PostCount,
                    CurrentMember = CurrentMember
                };

                // Get the topic view slug
                return View(PathHelper.GetThemeViewPath("MemberProfile"), viewModel);
            }
        }


        [HttpPost]
        [Authorize]
        public void ApproveMember(ApproveMemberViewModel model)
        {
            if (Request.IsAjaxRequest() && User.IsInRole(AppConstants.AdminRoleName))
            {
                try
                {
                    var member = MemberService.Get(model.Id);
                    MemberService.ApproveMember(member);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(PostChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var changePasswordSucceeded = MemberService.ResetPassword(CurrentMember, model.ChangePasswordViewModel.NewPassword);
                    if (changePasswordSucceeded)
                    {
                        ShowMessage(new GenericMessageViewModel
                        {
                            Message = Lang("Members.ChangePassword.Success"),
                            MessageType = GenericMessages.Success
                        });
                        return Redirect(Urls.GenerateUrl(Urls.UrlType.ChangePassword));
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
            ShowMessage(new GenericMessageViewModel
            {
                Message = Lang("Members.ChangePassword.Error"),
                MessageType = GenericMessages.Danger
            });
            return Redirect(Urls.GenerateUrl(Urls.UrlType.ChangePassword));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PostMemberEditViewModel userModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MemberService.Get(userModel.MemberEditViewModel.Id);
                var userEditUrl = $"{Urls.GenerateUrl(Urls.UrlType.EditMember)}?id={user.Id}";

                // Before we do anything DB wise, check it contains no bad links
                if (BannedLinkService.ContainsBannedLink(userModel.MemberEditViewModel.Signature) || BannedLinkService.ContainsBannedLink(userModel.MemberEditViewModel.Website))
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = Lang("Errors.BannedLink"),
                        MessageType = GenericMessages.Danger
                    });
                    return Redirect(userEditUrl);
                }

                // Sort image out first
                if (userModel.MemberEditViewModel.Files != null)
                {
                    // Before we save anything, check the user already has an upload folder and if not create one
                    //var uploadFolderPath = AppHelpers.GetMemberUploadPath(CurrentMember.Id);

                    // Loop through each file and get the file info and save to the users folder and Db
                    var file = userModel.MemberEditViewModel.Files[0];
                    if (file != null)
                    {
                        // If successful then upload the file
                        var memberMediFolderId = MemberService.ConfirmMemberAvatarMediaFolder();
                        var uploadResult = UploadedFileService.UploadFile(file, memberMediFolderId, true);

                        if (!uploadResult.UploadSuccessful)
                        {
                            ShowMessage(new GenericMessageViewModel
                            {
                                Message = uploadResult.ErrorMessage,
                                MessageType = GenericMessages.Danger
                            });
                            return Redirect(userEditUrl);
                        }


                        // Save avatar to user
                        user.Avatar = uploadResult.UploadedFileUrl;

                    }
                }

                user.Signature = BannedWordService.SanitiseBannedWords(AppHelpers.ScrubHtml(userModel.MemberEditViewModel.Signature));
                if (userModel.MemberEditViewModel.Twitter != null && userModel.MemberEditViewModel.Twitter.IndexOf("http", StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    user.Twitter = BannedWordService.SanitiseBannedWords(AppHelpers.SafePlainText(userModel.MemberEditViewModel.Twitter));
                }
                user.Website = BannedWordService.SanitiseBannedWords(AppHelpers.SafePlainText(userModel.MemberEditViewModel.Website));
                user.Comments = BannedWordService.SanitiseBannedWords(AppHelpers.SafePlainText(userModel.MemberEditViewModel.Comments));


                // User is trying to update their email address, need to 
                // check the email is not already in use
                if (userModel.MemberEditViewModel.Email != user.Email)
                {
                    // Add get by email address
                    var sanitisedEmail = AppHelpers.SafePlainText(userModel.MemberEditViewModel.Email);
                    var userWithSameEmail = MemberService.GetByEmail(sanitisedEmail);

                    //Firstly check new email isn't banned!
                    if (BannedEmailService.EmailIsBanned(sanitisedEmail))
                    {
                        unitOfWork.Rollback();
                        ModelState.AddModelError(string.Empty, Lang("Error.EmailIsBanned"));
                        ShowMessage();
                        return Redirect(userEditUrl);
                    }

                    // If the username doesn't match this user then someone else has this email address already
                    if (userWithSameEmail != null && userWithSameEmail.UserName != user.UserName)
                    {
                        unitOfWork.Rollback();
                        ModelState.AddModelError(string.Empty, Lang("Members.Errors.DuplicateEmail"));
                        ShowMessage();
                        return Redirect(userEditUrl);
                    }

                    user.Email = sanitisedEmail;
                }

                // User is trying to change username, need to check if a user already exists
                // with the username they are trying to change to
                var changedUsername = false;
                var sanitisedUsername = BannedWordService.SanitiseBannedWords(AppHelpers.SafePlainText(userModel.MemberEditViewModel.UserName));
                if (sanitisedUsername != user.UserName)
                {
                    if (MemberService.Get(sanitisedUsername) != null)
                    {
                        unitOfWork.Rollback();
                        ModelState.AddModelError(string.Empty, Lang("Members.Errors.DuplicateUserName"));
                        ShowMessage();
                        return Redirect(userEditUrl);
                    }

                    user.UserName = sanitisedUsername;
                    changedUsername = true;
                }

                // Update Everything
                MemberService.SaveMember(user, changedUsername);
                ActivityService.ProfileUpdated(user);

                ShowMessage(new GenericMessageViewModel
                {
                    Message = Lang("Member.ProfileUpdated"),
                    MessageType = GenericMessages.Success
                });

                try
                {

                    // Need to save member here

                    unitOfWork.Commit();

                    if (changedUsername)
                    {
                        // User has changed their username so need to log them in
                        // as there new username of 
                        var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        if (authCookie != null)
                        {
                            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                            if (authTicket != null)
                            {
                                var newFormsIdentity = new FormsIdentity(new FormsAuthenticationTicket(authTicket.Version,
                                                                                                       user.UserName,
                                                                                                       authTicket.IssueDate,
                                                                                                       authTicket.Expiration,
                                                                                                       authTicket.IsPersistent,
                                                                                                       authTicket.UserData));
                                var roles = authTicket.UserData.Split("|".ToCharArray());
                                var newGenericPrincipal = new GenericPrincipal(newFormsIdentity, roles);
                                System.Web.HttpContext.Current.User = newGenericPrincipal;
                            }
                        }

                        // sign out current user
                        FormsAuthentication.SignOut();

                        // Abandon the session
                        Session.Abandon();

                        // Sign in new user
                        FormsAuthentication.SetAuthCookie(user.UserName, false);
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LogError(ex);
                    ModelState.AddModelError(string.Empty, Lang("Errors.GenericMessage"));
                }

                ShowMessage();
                return Redirect(userEditUrl);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Report(ReportMemberViewModel viewModel)
        {
            if (Settings.EnableMemberReporting)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MemberService.Get(viewModel.MemberId);

                    // Banned link?
                    if (BannedLinkService.ContainsBannedLink(viewModel.Reason))
                    {
                        ShowMessage(new GenericMessageViewModel
                        {
                            Message = Lang("Errors.BannedLink"),
                            MessageType = GenericMessages.Danger
                        });
                        return Redirect(user.Url);
                    }

                    var report = new Report
                    {
                        Reason = viewModel.Reason,
                        ReportedMember = user,
                        Reporter = CurrentMember
                    };
                    ReportService.MemberReport(report, EmailService);
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = Lang("Report.ReportSent"),
                        MessageType = GenericMessages.Success
                    });
                    return Redirect(user.Url);
                }
            }
            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }

        [Authorize]
        public ActionResult KillSpammer()
        {
            //Check permission
            if (User.IsInRole(AppConstants.AdminRoleName) || CurrentMember.CanEditOtherMembers)
            {

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var id = Request["id"];
                    if (id != null)
                    {
                        // Get the member
                        var member = MemberService.Get(Convert.ToInt32(id));

                        // Delete all their posts and votes and delete etc..
                        var worked = MemberService.DeleteAllAssociatedMemberInfo(member.Id, unitOfWork, UploadedFileService, PostService, MemberPointsService, PollService, TopicService, TopicNotificationService, ActivityService, PrivateMessageService, BadgeService, VoteService, CategoryNotificationService);

                        // SAVE UOW
                        var message = new GenericMessageViewModel
                        {
                            Message = Lang("Member.SpammerIsKilled"),
                            MessageType = GenericMessages.Success
                        };

                        try
                        {
                            // Clear the website and signature fields and ban them
                            MemberService.KillSpammer(member);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex);
                            message.MessageType = GenericMessages.Danger;
                            message.Message = ex.Message;
                        }
                        ShowMessage(message);
                        return Redirect(member.Url);
                    }
                }


            }
            return ErrorToHomePage(Lang("Errors.NoPermission"));
        }

        [Authorize]
        public ActionResult BanMember()
        {
            //Check permission
            if (User.IsInRole(AppConstants.AdminRoleName) || CurrentMember.CanEditOtherMembers)
            {

                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var id = Request["id"];
                    if (id != null)
                    {
                        // Get the member
                        var member = MemberService.Get(Convert.ToInt32(id));

                        var message = new GenericMessageViewModel
                        {
                            Message = Lang("Member.IsBanned"),
                            MessageType = GenericMessages.Success
                        };

                        try
                        {
                            MemberService.BanMember(member);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex);
                            message.MessageType = GenericMessages.Danger;
                            message.Message = ex.Message;
                        }
                        ShowMessage(message);
                        return Redirect(member.Url);
                    }
                }
            }
            return ErrorToHomePage(Lang("Errors.NoPermission"));
        }

        [Authorize]
        public ActionResult UnBanMember()
        {
            //Check permission
            if (User.IsInRole(AppConstants.AdminRoleName) || CurrentMember.CanEditOtherMembers)
            {

                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var id = Request["id"];
                    if (id != null)
                    {
                        // Get the member
                        var member = MemberService.Get(Convert.ToInt32(id));

                        var message = new GenericMessageViewModel
                        {
                            Message = Lang("Member.IsUnBanned"),
                            MessageType = GenericMessages.Success
                        };

                        try
                        {
                            MemberService.UnBanMember(member);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex);
                            message.MessageType = GenericMessages.Danger;
                            message.Message = ex.Message;
                        }
                        ShowMessage(message);
                        return Redirect(member.Url);
                    }
                }
            }
            return ErrorToHomePage(Lang("Errors.NoPermission"));
        }

        [Authorize]
        public PartialViewResult SideAdminPanel()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var count = 0;
                if (CurrentMember != null)
                {
                    count = PrivateMessageService.NewPrivateMessageCount(CurrentMember.Id);
                }
                if (count > 0)
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = Lang("Member.HasNewPrivateMessages"),
                        MessageType = GenericMessages.Info
                    });
                }
                return PartialView(PathHelper.GetThemePartialViewPath("SideAdminPanel"),
                            new ViewAdminSidePanelViewModel { CurrentUser = CurrentMember, NewPrivateMessageCount = count });
            }
        }

        [HttpPost]
        public PartialViewResult GetMemberDiscussions(int id)
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    // Get the user discussions, only grab 100 posts
                    var posts = PostService.GetByMember(id, 100);

                    // Get the distinct topics
                    var topics = posts.Select(x => x.Topic).Where(x => x.Pending != true).Distinct().Take(6).OrderByDescending(x => x.LastPost.DateCreated).ToList();
                    TopicService.PopulateCategories(topics);

                    // Get all the categories for this topic collection
                    var categories = topics.Select(x => x.Category).Distinct();

                    // create the view model
                    var viewModel = new ViewMemberDiscussionsViewModel
                    {
                        Topics = topics,
                        AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                        CurrentUser = CurrentMember
                    };

                    // loop through the categories and get the permissions
                    foreach (var category in categories)
                    {
                        var permissionSet = PermissionService.GetPermissions(category, _membersGroup, MemberService, CategoryPermissionService);
                        viewModel.AllPermissionSets.Add(category, permissionSet);
                    }

                    return PartialView(PathHelper.GetThemePartialViewPath("GetMemberDiscussions"), viewModel);
                }
            }
            return null;
        }

        public JsonResult LastActiveCheck()
        {
            if (UserIsAuthenticated)
            {
                var rightNow = DateTime.UtcNow;
                var usersDate = CurrentMember.LastActiveDate;

                var span = rightNow.Subtract(usersDate);
                var totalMins = span.TotalMinutes;

                if (totalMins > AppConstants.TimeSpanInMinutesToDoCheck)
                {
                    // Update users last activity date so we can show the latest users online
                    CurrentMember.LastActiveDate = DateTime.UtcNow;

                    // Update
                    try
                    {
                        MemberService.UpdateLastActiveDate(CurrentMember);
                    }
                    catch (Exception ex)
                    {

                        LogError(ex);
                    }
                }
            }

            // You can return anything to reset the timer.
            return Json(new { Timer = "reset" }, JsonRequestBehavior.AllowGet);
        }
    }

}