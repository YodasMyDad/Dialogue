using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Activity;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Routes;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Dialogue.Logic.Controllers
{
    public class DialoguePageController : BaseRenderController
    {
        private readonly IMemberGroup _membersGroup;

        public DialoguePageController()
        {
            _membersGroup = (CurrentMember == null ? ServiceFactory.MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        /// <summary>
        /// Used to render virtual dialogue pages
        /// </summary>
        public ActionResult Show(RenderModel model, string pagename)
        {
            var tagPage = model.Content as DialogueVirtualPage;
            if (tagPage == null)
            {
                throw new InvalidOperationException("The RenderModel.Content instance must be of type " + typeof(DialogueVirtualPage));
            }

            // TODO - Must be a better way of doing this
            // TODO - I'd just like to be able to call the action based on the pagename

            var page = new DialoguePage(model.Content.Parent);

            // Show leaderboard
            if (pagename.ToLower().Contains(AppConstants.PageUrlLeaderboard))
            {
                return Leaderboard(page);
            }

            // Show latest topics rss
            if (pagename.ToLower().Contains(AppConstants.PageUrlTopicsRss))
            {
                return TopicsRss(page);
            }

            // Show latest activity rss
            if (pagename.ToLower().Contains(AppConstants.PageUrlActivityRss))
            {
                return ActivityRss(page);                
            }

            // Show latest category rss
            if (pagename.ToLower().Contains(AppConstants.PageUrlCategoryRss))
            {
                return CategoryRss(page);
            }

            // Show Badges
            if (pagename.ToLower().Contains(AppConstants.PageUrlBadges))
            {
                return Badges(page);
            }

            // Show Favourites
            if (pagename.ToLower().Contains(AppConstants.PageUrlFavourites))
            {
                return Favourites(page);
            }

            // Post Report
            if (pagename.ToLower().Contains(AppConstants.PageUrlPostReport))
            {
                return Report(page);
            }

            // Edit Post
            if (pagename.ToLower().Contains(AppConstants.PageUrlEditPost))
            {
                return EditPost(page);
            }

            // Private Messages Inbox
            if (pagename.ToLower().Contains(AppConstants.PageUrlMessageInbox))
            {
                return PrivateMessages(page);
            }

            // Private Messages Outbox
            if (pagename.ToLower().Contains(AppConstants.PageUrlMessageOutbox))
            {
                return PrivateMessagesSent(page);
            }

            // Private Messages Create
            if (pagename.ToLower().Contains(AppConstants.PageUrlCreatePrivateMessage))
            {
                return PrivateMessagesCreate(page);
            }

            // Private Messages View
            if (pagename.ToLower().Contains(AppConstants.PageUrlViewPrivateMessage))
            {
                return ViewPrivateMessage(page);
            }

            // We return null here as this actionresult is purely used
            // to display virtual dialogue pages
            return null;
        }

        [Authorize]
        public ActionResult PrivateMessages(DialoguePage page)
        {
            if (CurrentMember.DisablePrivateMessages)
            {
                var message = new GenericMessageViewModel
                {
                    Message = Lang("Errors.NoPermission"),
                    MessageType = GenericMessages.Danger
                };
                ShowMessage(message);
                return Redirect(Settings.ForumRootUrl);
            }
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = AppHelpers.ReturnCurrentPagingNo();
                var pagedMessages = ServiceFactory.PrivateMessageService.GetPagedReceivedMessagesByUser(pageIndex, AppConstants.PrivateMessageListSize, CurrentMember);
                var viewModel = new PageListPrivateMessageViewModel(page)
                {
                    ListPrivateMessageViewModel = new ListPrivateMessageViewModel
                    {
                        Messages = pagedMessages,
                        PageIndex = pageIndex,
                        TotalCount = pagedMessages.TotalCount
                    },
                    PageTitle = Lang("PM.ReceivedPrivateMessages")
                };
                return View(PathHelper.GetThemeViewPath("PrivateMessages"), viewModel);
            }
        }

        [Authorize]
        public ActionResult ViewPrivateMessage(DialoguePage page)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var id = Request["id"];
                var message = ServiceFactory.PrivateMessageService.Get(new Guid(id));

                if (message.MemberToId == CurrentMember.Id | message.MemberFromId == CurrentMember.Id)
                {
                    //Mark as read if this is the receiver of the message
                    if (message.MemberToId == CurrentMember.Id)
                    {
                        // Update message as read
                        message.IsRead = true;

                        // Get the sent version and update that too
                        var sentMessage = ServiceFactory.PrivateMessageService.GetMatchingSentPrivateMessage(message.Subject, message.DateSent, message.MemberFromId, message.MemberToId);
                        if (sentMessage != null)
                        {
                            sentMessage.IsRead = true;
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

                    return View(new ViewPrivateMessageViewModel { Message = message });
                }

                return ErrorToHomePage(Lang("Errors.NoPermission"));
            }
        }


        [Authorize]
        public ActionResult PrivateMessagesSent(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = AppHelpers.ReturnCurrentPagingNo();
                var pagedMessages = ServiceFactory.PrivateMessageService.GetPagedSentMessagesByUser(pageIndex, AppConstants.PrivateMessageListSize, CurrentMember);
                var viewModel = new ListPrivateMessageViewModel
                {
                    Messages = pagedMessages
                };
                return View(viewModel);
            }
        }

        [Authorize]
        public ActionResult PrivateMessagesCreate(DialoguePage page)
        {
            var to = Request["to"];
            var id = Request["id"];

            // Check if private messages are enabled
            if (!Settings.AllowPrivateMessages || CurrentMember.DisablePrivateMessages)
            {
                return ErrorToHomePage(Lang("Errors.GenericMessage"));
            }

            // Check flood control
            var lastMessage = ServiceFactory.PrivateMessageService.GetLastSentPrivateMessage(CurrentMember.Id);
            if (lastMessage != null && AppHelpers.TimeDifferenceInMinutes(DateTime.UtcNow, lastMessage.DateSent) < Settings.PrivateMessageFloodControl)
            {
                ShowMessage(new GenericMessageViewModel
                {
                    Message = Lang("PM.SendingToQuickly"),
                    MessageType = GenericMessages.Danger
                });
                return Redirect(Urls.GenerateUrl(Urls.UrlType.MessageInbox));
            }

            // Check outbox size
            var senderCount = ServiceFactory.PrivateMessageService.GetAllSentByUser(CurrentMember.Id).Count;
            if (senderCount > Settings.PrivateMessageInboxSize)
            {
                ShowMessage(new GenericMessageViewModel
                {
                    Message = Lang("PM.SentItemsOverCapcity"),
                    MessageType = GenericMessages.Danger
                });
                return Redirect(Urls.GenerateUrl(Urls.UrlType.MessageInbox));
            }

            var viewModel = new CreatePrivateMessageViewModel();

            // add the username to the to box if available
            if (to != null)
            {
                var userTo = ServiceFactory.MemberService.Get(Convert.ToInt32(to));
                viewModel.UserToUsername = userTo.UserName;
            }

            // See if this is a reply or not
            if (id != null)
            {
                var previousMessage = ServiceFactory.PrivateMessageService.Get(new Guid(id));
                // Its a reply, get the details
                viewModel.UserToUsername = previousMessage.MemberFrom.UserName;
                viewModel.Subject = previousMessage.Subject;
                viewModel.PreviousMessage = previousMessage.Message;
            }
            return View(viewModel);
        }

        [Authorize]
        public ActionResult EditPost(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Got to get a lot of things here as we have to check permissions
                // Get the post
                var id = Request["id"];
                var post = ServiceFactory.PostService.Get(new Guid(id));

                // Get the topic
                var topic = post.Topic;
                var category = ServiceFactory.CategoryService.Get(topic.CategoryId);

                // get the users permissions
                var permissions = ServiceFactory.PermissionService.GetPermissions(category, _membersGroup);

                if (post.MemberId == CurrentMember.Id || permissions[AppConstants.PermissionModerate].IsTicked)
                {
                    var viewModel = new EditPostViewModel { Content = Server.HtmlDecode(post.PostContent), Id = post.Id, Permissions = permissions };

                    // Now check if this is a topic starter, if so add the rest of the field
                    if (post.IsTopicStarter)
                    {
                        viewModel.Category = topic.CategoryId;
                        viewModel.IsLocked = topic.IsLocked;
                        viewModel.IsSticky = topic.IsSticky;
                        viewModel.IsTopicStarter = post.IsTopicStarter;


                        viewModel.Name = topic.Name;
                        viewModel.Categories = ServiceFactory.CategoryService.GetAllowedCategories(_membersGroup).ToList();
                        if (topic.Poll != null && topic.Poll.PollAnswers.Any())
                        {
                            // Has a poll so add it to the view model
                            viewModel.PollAnswers = topic.Poll.PollAnswers;
                        }
                    }

                    var pageViewModel = new EditPostPageViewModel(page)
                    {
                        EditPostViewModel = viewModel,
                        PageTitle = Lang("Post.EditPostPageTitle")
                    };

                    return View(PathHelper.GetThemeViewPath("EditPost"), pageViewModel);
                }
                return NoPermission(topic);
            }
        }

        [Authorize]
        public ActionResult Report(DialoguePage page)
        {
            if (Settings.EnableSpamReporting)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var id = Request["id"];
                    var post = ServiceFactory.PostService.Get(new Guid(id));
                    var viewModel = new ReportPostPageViewModel(page)
                    {
                        PostId = post.Id,
                        Post = post,
                        PostCreatorUsername = post.Member.UserName,
                        PageTitle = string.Concat(Lang("Report.ReportPostBy"), post.Member.UserName)
                    };
                    return View(PathHelper.GetThemeViewPath("PostReport"), viewModel);
                }
            }
            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }

        [Authorize]
        public ActionResult Favourites(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new ViewFavouritesViewModel(page)
                {
                    PageTitle = Lang("Favourites.PageTitle")
                };

                var postIds = ServiceFactory.FavouriteService.GetAllByMember(CurrentMember.Id).Select(x => x.PostId);
                var allPosts = ServiceFactory.PostService.Get(postIds.ToList());
                viewModel.Posts = allPosts;           
                return View(PathHelper.GetThemeViewPath("Favourites"), viewModel);
            }
        }

        public ActionResult Badges(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var allBadges = ServiceFactory.BadgeService.GetallBadges();

                var badgesListModel = new AllBadgesViewModel(page)
                {
                    AllBadges = allBadges,
                    PageTitle = Lang("Badge.AllBadges.PageTitle")
                };

                return View(PathHelper.GetThemeViewPath("Badges"), badgesListModel);
            }
        }

        public ActionResult Leaderboard(DialoguePage page)
        {
            page.PageTitle = Lang("Page.Leaderboard.PageTitle");
 
           return View(PathHelper.GetThemeViewPath("Leaderboard"), page);
        }

        public ActionResult Activity(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Set the page index
                var pageIndex = AppHelpers.ReturnCurrentPagingNo();

                // Get the topics
                var activities = ServiceFactory.ActivityService.GetPagedGroupedActivities(pageIndex, Settings.ActivitiesPerPage);

                // create the view model
                var viewModel = new AllRecentActivitiesViewModel(page)
                {
                    Activities = activities,
                    PageIndex = pageIndex,
                    TotalCount = activities.TotalCount,
                    PageTitle = Lang("Activity.PageTitle")
                };

                return View(PathHelper.GetThemeViewPath("Activity"), viewModel);
            }
        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public ActionResult TopicsRss(DialoguePage page)
        {
            //page.PageTitle = Lang("Page.TopicsRss.PageTitle");

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // get an rss lit ready
                var rssTopics = new List<RssItem>();

                // Get only the cats from this forum
                var cats = ServiceFactory.CategoryService.GetAll();

                // Get the latest topics
                var topics = ServiceFactory.TopicService.GetRecentRssTopics(AppConstants.ActiveTopicsListSize, cats.Select(x => x.Id).ToList());

                // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // create permissions
                var permissions = new Dictionary<Category, PermissionSet>();

                // loop through the categories and get the permissions
                foreach (var category in categories)
                {
                    var permissionSet = ServiceFactory.PermissionService.GetPermissions(category, _membersGroup);
                    permissions.Add(category, permissionSet);
                }

                // Now loop through the topics and remove any that user does not have permission for
                foreach (var topic in topics)
                {
                    // Get the permissions for this topic via its parent category
                    var permission = permissions[topic.Category];

                    // Add only topics user has permission to
                    if (!permission[AppConstants.PermissionDenyAccess].IsTicked)
                    {
                        if (topic.Posts.Any())
                        {
                            var firstOrDefault = topic.Posts.FirstOrDefault(x => x.IsTopicStarter);
                            if (firstOrDefault != null)
                                rssTopics.Add(new RssItem { Description = firstOrDefault.PostContent, Link = topic.Url, Title = topic.Name, PublishedDate = topic.CreateDate });
                        }
                    }
                }

                return new RssResult(rssTopics, Lang("Rss.LatestTopics.Title"), Lang("Rss.LatestTopics.Description"));
            }

        }


        public ActionResult CategoryRss(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // get an rss lit ready
                var rssTopics = new List<RssItem>();

                var catId = Request["id"];
                if (!string.IsNullOrEmpty(catId))
                {

                    // Get the category
                    var category = ServiceFactory.CategoryService.Get(Convert.ToInt32(catId));

                    // check the user has permission to this category
                    var permissions = ServiceFactory.PermissionService.GetPermissions(category, _membersGroup);

                    if (!permissions[AppConstants.PermissionDenyAccess].IsTicked)
                    {
                        var topics = ServiceFactory.TopicService.GetRssTopicsByCategory(AppConstants.ActiveTopicsListSize, category.Id);

                        rssTopics.AddRange(topics.Select(x =>
                        {
                            var firstOrDefault =
                                x.Posts.FirstOrDefault(s => s.IsTopicStarter);
                            return firstOrDefault != null
                                       ? new RssItem
                                       {
                                           Description = firstOrDefault.PostContent,
                                           Link = x.Url,
                                           Title = x.Name,
                                           PublishedDate = x.CreateDate
                                       }
                                       : null;
                        }
                                               ));

                        return new RssResult(rssTopics, string.Format(Lang("Rss.Category.Title"), category.Name),
                                             string.Format(Lang("Rss.Category.Description"), category.Name));
                    }
                }

                return ErrorToHomePage(Lang("Errors.NothingToDisplay"));
            }
        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public ActionResult ActivityRss(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // get an rss lit ready
                var rssActivities = new List<RssItem>();

                var activities = ServiceFactory.ActivityService.GetAll(20).OrderByDescending(x => x.ActivityMapped.Timestamp);

                var activityLink = Urls.GenerateUrl(Urls.UrlType.Activity);

                // Now loop through the topics and remove any that user does not have permission for
                foreach (var activity in activities)
                {
                    if (activity is BadgeActivity)
                    {
                        var badgeActivity = activity as BadgeActivity;
                        rssActivities.Add(new RssItem
                        {
                            Description = badgeActivity.Badge.Description,
                            Title = string.Concat(badgeActivity.User.UserName, " ", Lang("Activity.UserAwardedBadge"), " ", badgeActivity.Badge.DisplayName, " ", Lang("Activity.Badge")),
                            PublishedDate = badgeActivity.ActivityMapped.Timestamp,
                            RssImage = AppHelpers.ReturnBadgeUrl(badgeActivity.Badge.Image),
                            Link = activityLink
                        });
                    }
                    else if (activity is MemberJoinedActivity)
                    {
                        var memberJoinedActivity = activity as MemberJoinedActivity;
                        rssActivities.Add(new RssItem
                        {
                            Description = string.Empty,
                            Title = Lang("Activity.UserJoined"),
                            PublishedDate = memberJoinedActivity.ActivityMapped.Timestamp,
                            RssImage = memberJoinedActivity.User.MemberImage(AppConstants.GravatarPostSize),
                            Link = activityLink
                        });
                    }
                    else if (activity is ProfileUpdatedActivity)
                    {
                        var profileUpdatedActivity = activity as ProfileUpdatedActivity;
                        rssActivities.Add(new RssItem
                        {
                            Description = string.Empty,
                            Title = Lang("Activity.ProfileUpdated"),
                            PublishedDate = profileUpdatedActivity.ActivityMapped.Timestamp,
                            RssImage = profileUpdatedActivity.User.MemberImage(AppConstants.GravatarPostSize),
                            Link = activityLink
                        });
                    }

                }

                return new RssResult(rssActivities, Lang("Rss.LatestActivity.Title"), Lang("Rss.LatestActivity.Description"));
            }
        }
    }
}