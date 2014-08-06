using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Dialogue.Logic.Application.Akismet;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Web;

namespace Dialogue.Logic.Controllers
{
    #region MVC Controllers
    public class DialoguePostSurfaceController : BaseSurfaceController
    {

        [HttpPost]
        public PartialViewResult CreatePost(CreateAjaxPostViewModel post)
        {
            PermissionSet permissions;
            Post newPost;
            Topic topic;
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Quick check to see if user is locked out, when logged in
                if (CurrentMember.IsLockedOut | !CurrentMember.IsApproved)
                {
                    ServiceFactory.MemberService.LogOff();
                    throw new Exception(Lang("Errors.NoAccess"));
                }

                topic = ServiceFactory.TopicService.Get(post.Topic);

                var postContent = ServiceFactory.BannedWordService.SanitiseBannedWords(post.PostContent);

                var akismetHelper = new AkismetHelper();

                newPost = ServiceFactory.PostService.AddNewPost(postContent, topic, CurrentMember, out permissions);

                if (!akismetHelper.IsSpam(newPost))
                {
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
                else
                {
                    unitOfWork.Rollback();
                    throw new Exception(Lang("Errors.PossibleSpam"));
                }
            }

            //Check for moderation
            if (newPost.Pending)
            {
                return PartialView(PathHelper.GetThemePartialViewPath("PostModeration"));
            }

            // All good send the notifications and send the post back
            using (UnitOfWorkManager.NewUnitOfWork())
            {

                // Create the view model
                var viewModel = new ViewPostViewModel
                {
                    Permissions = permissions,
                    Post = newPost,
                    User = CurrentMember,
                    ParentTopic = topic
                };

                // Success send any notifications

                NotifyNewTopics(topic);

                return PartialView(PathHelper.GetThemePartialViewPath("Post"), viewModel);
            }
        }

        private void NotifyNewTopics(Topic topic)
        {
            // Get all notifications for this category
            var notifications = ServiceFactory.TopicNotificationService.GetByTopic(topic).Select(x => x.MemberId).ToList();

            if (notifications.Any())
            {
                // remove the current user from the notification, don't want to notify yourself that you 
                // have just made a topic!
                notifications.Remove(CurrentMember.Id);

                if (notifications.Count > 0)
                {
                    // Now get all the users that need notifying
                    var usersToNotify = ServiceFactory.MemberService.GetUsersById(notifications);

                    // Create the email
                    var sb = new StringBuilder();
                    sb.AppendFormat("<p>{0}</p>", string.Format(Lang("Post.Notification.NewPosts"), topic.Name));
                    sb.AppendFormat("<p>{0}</p>", string.Concat(Settings.ForumRootUrlWithDomain, topic.Url));

                    // create the emails only to people who haven't had notifications disabled
                    var emails = usersToNotify.Where(x => x.DisableEmailNotifications != true).Select(user => new Email
                    {
                        Body = ServiceFactory.EmailService.EmailTemplate(user.UserName, sb.ToString()),
                        EmailFrom = Settings.NotificationReplyEmailAddress,
                        EmailTo = user.Email,
                        NameTo = user.UserName,
                        Subject = string.Concat(Lang("Post.Notification.Subject"), Settings.ForumName)
                    }).ToList();

                    // and now pass the emails in to be sent
                    ServiceFactory.EmailService.SendMail(emails);
                }
            }
        }
    } 
    #endregion
}