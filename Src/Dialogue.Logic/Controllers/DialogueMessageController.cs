using System;
using System.Text;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.Controllers
{
    #region Base Controllers
    public partial class DialogueMessageSurfaceController : BaseSurfaceController
    {
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreatePrivateMessageViewModel createPrivateMessageViewModel)
        {
            if (!Settings.AllowPrivateMessages || CurrentMember.DisablePrivateMessages)
            {
                return ErrorToHomePage(Lang("Errors.GenericMessage"));
            }
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (ModelState.IsValid)
                {
                    var userTo = createPrivateMessageViewModel.UserToUsername;

                    // first check they are not trying to message themself!
                    if (!string.Equals(userTo, CurrentMember.UserName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Map the view model to message
                        var privateMessage = new PrivateMessage
                        {
                            MemberFrom = CurrentMember,
                            MemberFromId = CurrentMember.Id,
                            Subject = createPrivateMessageViewModel.Subject,
                            Message = createPrivateMessageViewModel.Message,
                        };
                        // now get the user its being sent to
                        var memberTo = ServiceFactory.MemberService.Get(userTo);

                        // check the member
                        if (memberTo != null)
                        {

                            // Check in box size
                            // First check sender
                            var receiverCount = ServiceFactory.PrivateMessageService.GetAllReceivedByUser(memberTo.Id).Count;
                            if (receiverCount > Settings.PrivateMessageInboxSize)
                            {
                                ModelState.AddModelError(string.Empty, string.Format(Lang("PM.ReceivedItemsOverCapcity"), memberTo.UserName));
                            }
                            else
                            {
                                // Good to go send the message!
                                privateMessage.MemberTo = memberTo;
                                privateMessage.MemberToId = memberTo.Id;
                                ServiceFactory.PrivateMessageService.Add(privateMessage);

                                try
                                {
                                    ShowMessage(new GenericMessageViewModel
                                    {
                                        Message = Lang("PM.MessageSent"),
                                        MessageType = GenericMessages.Success
                                    });

                                    unitOfWork.Commit();

                                    // Finally send an email to the user so they know they have a new private message
                                    // As long as they have not had notifications disabled
                                    if (memberTo.DisableEmailNotifications != true)
                                    {
                                        var email = new Email
                                        {
                                            EmailFrom = Settings.NotificationReplyEmailAddress,
                                            EmailTo = memberTo.Email,
                                            Subject = Lang("PM.NewPrivateMessageSubject"),
                                            NameTo = memberTo.UserName
                                        };

                                        var sb = new StringBuilder();
                                        sb.AppendFormat("<p>{0}</p>", string.Format(Lang("PM.NewPrivateMessageBody"), CurrentMember.UserName));
                                        email.Body = ServiceFactory.EmailService.EmailTemplate(email.NameTo, sb.ToString());
                                        ServiceFactory.EmailService.SendMail(email);
                                    }

                                    return Redirect(Urls.GenerateUrl(Urls.UrlType.MessageInbox));
                                }
                                catch (Exception ex)
                                {
                                    unitOfWork.Rollback();
                                    LogError(ex);
                                    ModelState.AddModelError(string.Empty, Lang("Errors.GenericMessage"));
                                }
                            }
                        }
                        else
                        {
                            // Error send back to user
                            ModelState.AddModelError(string.Empty, Lang("PM.UnableFindMember"));
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, Lang("PM.TalkToSelf"));
                    }
                }
                ShowModelErrors();
                return Redirect(Urls.GenerateUrl(Urls.UrlType.MessageCreate));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(DeletePrivateMessageViewModel deletePrivateMessageViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (Request.IsAjaxRequest())
                {
                    var privateMessage = ServiceFactory.PrivateMessageService.Get(deletePrivateMessageViewModel.Id);
                    if (privateMessage.MemberToId == CurrentMember.Id | privateMessage.MemberFromId == CurrentMember.Id)
                    {
                        ServiceFactory.PrivateMessageService.DeleteMessage(privateMessage);
                    }
                    else
                    {
                        throw new Exception(Lang("Errors.NoPermission"));
                    }
                }

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LogError(ex);
                    throw new Exception(Lang("Errors.GenericMessage"));
                }
            }

            return null;
        }
    } 
    #endregion
}