using System;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Application.Akismet;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Routes;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using System.Collections.Generic;
using System.Text;

namespace Dialogue.Logic.Controllers
{
    public class DialogueTopicController : BaseController
    {
        private readonly CategoryService _categoryService;
        private readonly IMemberGroup _membersGroup;
        private readonly EmailService _emailService;
        private readonly TopicService _topicService;

        public DialogueTopicController()
        {
            _categoryService = new CategoryService();
            _membersGroup = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
            _emailService = new EmailService();
            _topicService = new TopicService();
        }

        /// <summary>
        /// Used to render the Topic (virtual node)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="topicname">
        /// The topic slug which we use to look up the topic
        /// </param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ActionResult Show(RenderModel model, string topicname, int? p = null)
        {
            var tagPage = model.Content as DialogueVirtualPage;
            if (tagPage == null)
            {
                throw new InvalidOperationException("The RenderModel.Content instance must be of type " + typeof(DialogueVirtualPage));
            }

            //create a blog model of the main page
            var viewModel = new ShowTopicViewModel(model.Content)
            {
                Topic = _topicService.GetTopicBySlug(topicname)
            };

            // Get the topic view slug


            return View(PathHelper.GetThemeViewPath("Topic"), viewModel);
        }


    }


    public partial class DialogueCreateTopicController : BaseController
    {
        private readonly MemberPointsService _memberPointsService;
        private readonly PrivateMessageService _privateMessageService;
        private readonly CategoryService _categoryService;
        private readonly IMemberGroup UsersRole;
        private readonly BannedEmailService _bannedEmailService;
        private readonly BannedWordService _bannedWordService;
        private readonly TopicNotificationService _topicNotificationService;
        private readonly CategoryNotificationService _categoryNotificationService;
        private readonly EmailService _emailService;
        private readonly IMemberGroup _membersGroup;

        public DialogueCreateTopicController()
        {
            _privateMessageService = new PrivateMessageService();
            _categoryService = new CategoryService();
            UsersRole = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
            _bannedEmailService = new BannedEmailService();
            _bannedWordService = new BannedWordService();
            _memberPointsService = new MemberPointsService();
            _topicNotificationService = new TopicNotificationService();
            _categoryNotificationService = new CategoryNotificationService();
            _emailService = new EmailService();
            _membersGroup = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        public override ActionResult Index(RenderModel model)
        {
            if (UserIsAuthenticated)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var allowedCategories = _categoryService.GetAllowedCategories(_membersGroup).ToList();
                    if (allowedCategories.Any() && CurrentMember.DisablePosting != true)
                    {
                        var viewModel = new CreateTopic(model.Content)
                        {
                            Categories = allowedCategories,
                            LoggedOnUser = CurrentMember,
                            PageTitle = Lang("Topic.CreateTopic")
                        };

                        return View(PathHelper.GetThemeViewPath("Create"), viewModel);
                    }
                }
            }
            return ErrorToHomePage(Lang("Errors.NoPermission"));
        }

    }

    public partial class DialogueTopicSurfaceController : BaseSurfaceController
    {
        private readonly MemberPointsService _memberPointsService;
        private readonly PrivateMessageService _privateMessageService;
        private readonly CategoryService _categoryService;
        private readonly IMemberGroup UsersRole;
        private readonly BannedEmailService _bannedEmailService;
        private readonly BannedWordService _bannedWordService;
        private readonly TopicService _topicService;
        private readonly TopicNotificationService _topicNotificationService;
        private readonly PollService _pollService;
        private readonly CategoryNotificationService _categoryNotificationService;
        private readonly EmailService _emailService;
        
        public DialogueTopicSurfaceController()
        {
            _privateMessageService = new PrivateMessageService();
            _categoryService = new CategoryService();
            UsersRole = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
            _bannedEmailService = new BannedEmailService();
            _bannedWordService = new BannedWordService();
            _memberPointsService = new MemberPointsService();
            _topicNotificationService = new TopicNotificationService();
            _categoryNotificationService = new CategoryNotificationService();
            _topicService = new TopicService();
            _pollService = new PollService();
            _emailService = new EmailService();
        }


        public PartialViewResult LatestTopics(int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Set the page index
                var pageIndex = p ?? 1;

                // Get the topics
                var topics = _topicService.GetRecentTopics(pageIndex,
                                                           Dialogue.Settings().TopicsPerPage,
                                                           AppConstants.ActiveTopicsListSize);

                // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // create the view model
                var viewModel = new ActiveTopicsViewModel
                {
                    Topics = topics,
                    AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                    PageIndex = pageIndex,
                    TotalCount = topics.TotalCount,
                    User = CurrentMember
                };

                // loop through the categories and get the permissions
                foreach (var category in categories)
                {
                    var permissionSet = PermissionService.GetPermissions(category, UsersRole);
                    viewModel.AllPermissionSets.Add(category, permissionSet);
                }
                return PartialView(PathHelper.GetThemePartialViewPath("LatestTopics"), viewModel);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateTopicViewModel topicViewModel)
        {
            if (ModelState.IsValid)
            {
                // Quick check to see if user is locked out, when logged in
                if (CurrentMember.IsLockedOut || CurrentMember.DisablePosting == true || !CurrentMember.IsApproved)
                {
                    MemberService.LogOff();
                    return ErrorToHomePage(Lang("Errors.NoPermission"));
                }

                var successfullyCreated = false;
                var moderate = false;
                Category category;
                var topic = new Topic();

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // Not using automapper for this one only, as a topic is a post and topic in one
                    category = _categoryService.Get(topicViewModel.Category);

                    // First check this user is allowed to create topics in this category
                    var permissions = PermissionService.GetPermissions(category, UsersRole);

                    // Check this users role has permission to create a post
                    if (permissions[AppConstants.PermissionDenyAccess].IsTicked || permissions[AppConstants.PermissionReadOnly].IsTicked || !permissions[AppConstants.PermissionCreateTopics].IsTicked)
                    {
                        // Throw exception so Ajax caller picks it up
                        ModelState.AddModelError(string.Empty, Lang("Errors.NoPermission"));
                    }
                    else
                    {
                        // We get the banned words here and pass them in, so its just one call
                        // instead of calling it several times and each call getting all the words back
                        var bannedWordsList = _bannedWordService.GetAll();
                        List<string> bannedWords = null;
                        if (bannedWordsList.Any())
                        {
                            bannedWords = bannedWordsList.Select(x => x.Word).ToList();
                        }

                        topic = new Topic
                        {
                            Name = _bannedWordService.SanitiseBannedWords(topicViewModel.TopicName, bannedWords),
                            Category = category,
                            CategoryId = category.Id,
                            Member = CurrentMember,
                            MemberId = CurrentMember.Id
                        };

                        // See if the user has actually added some content to the topic
                        if (!string.IsNullOrEmpty(topicViewModel.TopicContent))
                        {
                            // Check for any banned words
                            topicViewModel.TopicContent = _bannedWordService.SanitiseBannedWords(topicViewModel.TopicContent, bannedWords);

                            // See if this is a poll and add it to the topic
                            if (topicViewModel.PollAnswers != null && topicViewModel.PollAnswers.Count > 0)
                            {
                                // Do they have permission to create a new poll
                                if (permissions[AppConstants.PermissionCreatePolls].IsTicked)
                                {
                                    // Create a new Poll
                                    var newPoll = new Poll
                                    {
                                        Member = CurrentMember,
                                        MemberId = CurrentMember.Id
                                    };

                                    // Create the poll
                                    _pollService.Add(newPoll);

                                    // Save the poll in the context so we can add answers
                                    unitOfWork.SaveChanges();

                                    // Now sort the answers
                                    var newPollAnswers = new List<PollAnswer>();
                                    foreach (var pollAnswer in topicViewModel.PollAnswers)
                                    {
                                        // Attach newly created poll to each answer
                                        pollAnswer.Poll = newPoll;
                                        _pollService.Add(pollAnswer);
                                        newPollAnswers.Add(pollAnswer);
                                    }
                                    // Attach answers to poll
                                    newPoll.PollAnswers = newPollAnswers;

                                    // Save the new answers in the context
                                    unitOfWork.SaveChanges();

                                    // Add the poll to the topic
                                    topic.Poll = newPoll;
                                }
                                else
                                {
                                    //No permission to create a Poll so show a message but create the topic
                                    ShowMessage(new GenericMessageViewModel
                                    {
                                        Message = Lang("Errors.NoPermissionPolls"),
                                        MessageType = GenericMessages.Info
                                    });
                                }
                            }

                            // Update the users points score for posting
                            _memberPointsService.Add(new MemberPoints
                            {
                                Points = Settings.PointsAddedPerNewPost,
                                Member = CurrentMember,
                                MemberId = CurrentMember.Id
                            });

                            // Check for moderation
                            if (category.ModerateAllTopicsInThisCategory == true)
                            {
                                topic.Pending = true;
                                moderate = true;
                            }

                            // Create the topic
                            topic = _topicService.Add(topic);

                            // Save the changes
                            unitOfWork.SaveChanges();

                            // Now create and add the post to the topic
                            _topicService.AddLastPost(topic, topicViewModel.TopicContent);

                            // Now check its not spam
                            var akismetHelper = new AkismetHelper();
                            if (!akismetHelper.IsSpam(topic))
                            {
                                // Subscribe the user to the topic as they have checked the checkbox
                                if (topicViewModel.SubscribeToTopic)
                                {
                                    // Create the notification
                                    var topicNotification = new TopicNotification
                                    {
                                        Topic = topic,
                                        Member = CurrentMember,
                                        MemberId = CurrentMember.Id
                                    };
                                    //save
                                    _topicNotificationService.Add(topicNotification);
                                }

                                try
                                {
                                    unitOfWork.Commit();
                                    if (!moderate)
                                    {
                                        successfullyCreated = true;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    unitOfWork.Rollback();
                                    LogError(ex);
                                    ModelState.AddModelError(string.Empty, Lang("Errors.GenericMessage"));
                                }
                            }
                            else
                            {
                                unitOfWork.Rollback();
                                ModelState.AddModelError(string.Empty, Lang("Errors.PossibleSpam"));
                            }

                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, Lang("Errors.GenericMessage"));
                        }
                    }
                }

                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    if (successfullyCreated)
                    {
                        // Success so now send the emails
                        NotifyNewTopics(category);

                        // Redirect to the newly created topic
                        return Redirect(string.Format("{0}?postbadges=true", topic.NiceUrl));
                    }
                    if (moderate)
                    {
                        // Moderation needed
                        // Tell the user the topic is awaiting moderation
                        return MessageToHomePage(Lang("Moderate.AwaitingModeration"));
                    }
                }
            }
            ShowModelErrors();
            return CurrentUmbracoPage();
        }

        private void NotifyNewTopics(Category cat)
        {
            // *CHANGE THIS TO BE CALLED LIKE THE BADGES VIA AN AJAX Method* 
            // TODO: This really needs to be an async call so it doesn't hang when a user creates  
            //  a topic if there are 1000's of users

            // Get all notifications for this category
            var notifications = _categoryNotificationService.GetByCategory(cat).Select(x => x.MemberId).ToList();

            if (notifications.Any())
            {
                // remove the current user from the notification, don't want to notify yourself that you 
                // have just made a topic!
                notifications.Remove(CurrentMember.Id);

                if (notifications.Count > 0)
                {
                    // Now get all the users that need notifying
                    var usersToNotify = MemberService.GetUsersById(notifications);

                    // Create the email
                    var sb = new StringBuilder();
                    sb.AppendFormat("<p>{0}</p>", string.Format(Lang("Topic.Notification.NewTopics"), cat.Name));
                    sb.AppendFormat("<p>{0}</p>", string.Concat(Settings.ForumRootUrlWithDomain, cat.Url));

                    // create the emails and only send them to people who have not had notifications disabled
                    var emails = usersToNotify.Where(x => x.DisableEmailNotifications != true).Select(user => new Email
                    {
                        Body = _emailService.EmailTemplate(user.UserName, sb.ToString()),
                        EmailFrom = Settings.NotificationReplyEmailAddress,
                        EmailTo = user.Email,
                        NameTo = user.UserName,
                        Subject = string.Concat(Lang("Topic.Notification.Subject"), Settings.ForumName)
                    }).ToList();

                    // and now pass the emails in to be sent
                    _emailService.SendMail(emails);
                }
            }
        }


        public PartialViewResult CreateTopicButton()
        {
            var viewModel = new CreateTopicButtonViewModel
            {
                LoggedOnUser = CurrentMember
            };

            if (CurrentMember != null)
            {
                // Add all categories to a permission set
                var allCategories = _categoryService.GetAll();
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    foreach (var category in allCategories)
                    {
                        // Now check to see if they have access to any categories
                        // if so, check they are allowed to create topics - If no to either set to false
                        viewModel.UserCanPostTopics = false;
                        var permissionSet = PermissionService.GetPermissions(category, UsersRole);
                        if (permissionSet[AppConstants.PermissionCreateTopics].IsTicked)
                        {
                            viewModel.UserCanPostTopics = true;
                            break;
                        }
                    }
                }
            }
            return PartialView(PathHelper.GetThemePartialViewPath("CreateTopicButton"), viewModel);
        }


    }
}