namespace Dialogue.Logic.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;
    using Application;
    using Constants;
    using Mapping;
    using Models;
    using Models.Activity;
    using Models.ViewModels;
    using Routes;
    using Umbraco.Core.Models;
    using Umbraco.Web.Models;

    public partial class DialoguePageController : DialogueBaseController
    {
        private readonly IMemberGroup _membersGroup;

        public DialoguePageController()
        {
            _membersGroup = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
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
            // TODO - I'd just like to be able to call the action with a string based on the pagename
            // TODO - Like Action("ActionName", viewModel) ??

            var page = new DialoguePage(model.Content.Parent);
            var pageLowered = pagename.ToLower();

            switch (pageLowered)
            {
                case AppConstants.PageUrlLeaderboard:
                    return Leaderboard(page);

                case AppConstants.PageUrlTopicsRss:
                    return TopicsRss(page);

                case AppConstants.PageUrlActivityRss:
                    return ActivityRss(page);

                case AppConstants.PageUrlActivity:
                    return Activity(page);

                case AppConstants.PageUrlCategoryRss:
                    return CategoryRss(page);

                case AppConstants.PageUrlBadges:
                    return Badges(page);

                case AppConstants.PageUrlFavourites:
                    return Favourites(page);

                case AppConstants.PageUrlPostReport:
                    return Report(page);

                case AppConstants.PageUrlEditPost:
                    return EditPost(page);

                case AppConstants.PageUrlMessageInbox:
                    return PrivateMessages(page);

                case AppConstants.PageUrlMessageOutbox:
                    return PrivateMessagesSent(page);

                case AppConstants.PageUrlCreatePrivateMessage:
                    return PrivateMessagesCreate(page);

                case AppConstants.PageUrlViewPrivateMessage:
                    return ViewPrivateMessage(page);

                case AppConstants.PageUrlViewReportMember:
                    return ReportMember(page);

                case AppConstants.PageUrlEditMember:
                    return EditMember(page);

                case AppConstants.PageUrlChangePassword:
                    return ChangePassword(page);

                case AppConstants.PageUrlSearch:
                    return Search(page);

                case AppConstants.PageUrlCreateTopic:
                    return Create(page);

                case AppConstants.PageUrlEmailConfirmation:
                    return EmailConfirmation(page);

                case AppConstants.PageUrlSpamOverview:
                    return SpamOverview(page);

                case AppConstants.PageUrlAuthorise:
                    return Authorise(page);
            }

            return ErrorToHomePage(Lang("Errors.GenericMessage"));

        }

        public ActionResult Authorise(DialoguePage page)
        {
            if (User.IsInRole(AppConstants.AdminRoleName))
            {
                var viewModel = new AuthoriseViewModel
                {
                    PageTitle = Lang("Authorise.PageTitle"),
                    Members = MemberService.GetUnAuthorisedMembers(),
                    Posts = PostService.GetAllPendingPosts(),
                    Topics = TopicService.GetAllPendingTopics()
                };

                return View(PathHelper.GetThemeViewPath("Authorise"), viewModel);
            }
            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }

        public ActionResult SpamOverview(DialoguePage page)
        {
            if (User.IsInRole(AppConstants.AdminRoleName))
            {
                var viewModel = new SpamOverviewModel
                {
                    DodgyMembers = MemberPointsService.GetLatestNegativeUsers(Settings.PostsPerPage),
                    DodgyPosts = PostService.GetLowestVotedPost(Settings.PostsPerPage),
                    PageTitle = Lang("Spam.OverViewPageTitle")
                };

                return View(PathHelper.GetThemeViewPath("SpamOverview"), viewModel);   
            }
            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }


        public ActionResult EmailConfirmation(DialoguePage page)
        {
            var id = Request["id"];
            if (id != null)
            {
                    try
                    {
                        var user = MemberService.Get(Convert.ToInt32(id));

                        // Checkconfirmation
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
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }
     
            }

            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }

        public ActionResult Create(DialoguePage page)
        {
            if (UserIsAuthenticated)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var allowedCategories = CategoryService.GetAllowedCategories(_membersGroup, PermissionService, MemberService, CategoryPermissionService).ToList();
                    if (allowedCategories.Any() && CurrentMember.DisablePosting != true)
                    {
                        var viewModel = new CreateTopic(page)
                        {
                            Categories = allowedCategories,
                            LoggedOnUser = CurrentMember,
                            SubscribeToTopic = true,
                            PageTitle = Lang("Topic.CreateTopic")
                        };

                        // Pre-Select category user is in
                        var cat = Request["cat"];
                        if (!string.IsNullOrEmpty(cat))
                        {
                            var catId = Convert.ToInt32(cat);
                            if (catId > 0)
                            {
                                viewModel.Category = catId;
                            }
                        }

                        return View(PathHelper.GetThemeViewPath("Create"), viewModel);
                    }
                }
            }
            return ErrorToHomePage(Lang("Errors.NoPermission"));
        }

        public ActionResult Search(DialoguePage page)
        {
            var term = Request["term"];
            if (!string.IsNullOrEmpty(term))
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    // Set the page index
                    var pageIndex = AppHelpers.ReturnCurrentPagingNo();

                    // Returns the formatted string to search on
                    var formattedSearchTerm = AppHelpers.ReturnSearchString(term);

                    // Create an empty viewmodel
                    var viewModel = new SearchViewModel(page)
                    {
                        Posts = new List<ViewPostViewModel>(),
                        AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                        PageIndex = pageIndex,
                        TotalCount = 0,
                        Term = term
                    };

                    // if there are no results from the filter return an empty search view model.
                    if (string.IsNullOrWhiteSpace(formattedSearchTerm))
                    {
                        return View(PathHelper.GetThemeViewPath("Search"), viewModel);
                    }

                    //// Get all the topics based on the search value
                    var posts = PostService.SearchPosts(pageIndex,
                                                         Settings.PostsPerPage,
                                                         AppConstants.ActiveTopicsListSize,
                                                         term);


                    // Get all the categories for this topic collection
                    var topics = posts.Select(x => x.Topic).Distinct().ToList();
                    var categoryIds = topics.Select(x => x.CategoryId).Distinct().ToList();
                    var categories = CategoryService.Get(categoryIds);

                    // create the view model
                    viewModel = new SearchViewModel(page)
                    {
                        AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                        PageIndex = pageIndex,
                        TotalCount = posts.TotalCount,
                        Term = formattedSearchTerm,
                        TotalPages = posts.TotalPages
                    };

                    // Current members favourites
                    var favourites = new List<Favourite>();
                    if (CurrentMember != null)
                    {
                        favourites = FavouriteService.GetAllByMember(CurrentMember.Id);
                    }

                    // Get all votes for all the posts
                    var postIds = posts.Select(x => x.Id).ToList();
                    var allPostVotes = VoteService.GetAllVotesForPosts(postIds);

                    // loop through the categories and get the permissions
                    foreach (var category in categories)
                    {
                        var permissionSet = PermissionService.GetPermissions(category, _membersGroup, MemberService, CategoryPermissionService);
                        viewModel.AllPermissionSets.Add(category, permissionSet);
                    }

                    // Map the posts to the posts viewmodel
                    viewModel.Posts = new List<ViewPostViewModel>();
                    foreach (var post in posts)
                    {
                        // TODO - See if we can make this more efficient
                        var cat = categories.FirstOrDefault(x => x.Id == post.Topic.CategoryId);
                        var postViewModel = PostMapper.MapPostViewModel(viewModel.AllPermissionSets[cat], post, CurrentMember, Settings, post.Topic, allPostVotes, favourites, true);
                        viewModel.Posts.Add(postViewModel);
                    }

                    viewModel.PageTitle = string.Concat(Lang("Search.PageTitle"), AppHelpers.SafePlainText(term));

                    return View(PathHelper.GetThemeViewPath("Search"), viewModel);
                }
            }

            return Redirect(Settings.ForumRootUrl);
        }

        [Authorize]
        public ActionResult ChangePassword(DialoguePage page)
        {

            var viewModel = new PageChangePasswordViewModel(page)
            {
                PageTitle = Lang("Members.ChangePassword.Title")
            };
            return View(PathHelper.GetThemeViewPath("ChangePassword"), viewModel);
        }

        [Authorize]
        public ActionResult EditMember(DialoguePage page)
        {
            var id = Request["id"];
            if (id != null)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MemberService.Get(Convert.ToInt32(id));
                    var viewModel = new PageMemberEditViewModel(page)
                    {
                        MemberEditViewModel = new MemberEditViewModel
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            Signature = user.Signature,
                            Website = user.Website,
                            Twitter = user.Twitter,
                            Avatar = user.Avatar,
                            Comments = user.Comments,

                            DisableFileUploads = user.DisableFileUploads,
                            DisableEmailNotifications = user.DisableEmailNotifications,
                            DisablePosting = user.DisablePosting,
                            DisablePrivateMessages = user.DisablePrivateMessages,
                            CanEditOtherMembers = user.CanEditOtherMembers
                        },
                        PageTitle = string.Format(Lang("Members.EditProfile"), user.UserName)
                    };

                    return View(PathHelper.GetThemeViewPath("EditMember"), viewModel);
                }
            }

            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }



        [Authorize]
        public ActionResult ReportMember(DialoguePage page)
        {
            var id = Request["id"];
            if (string.IsNullOrEmpty(id))
            {
                return ErrorToHomePage(Lang("Errors.GenericMessage"));
            }
            if (Settings.EnableMemberReporting)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MemberService.Get(Convert.ToInt32(id));
                    var viewModel = new PageReportMemberViewModel(page)
                    {
                        MemberId = user.Id, 
                        Username = user.UserName,
                        PageTitle = Lang("Report.MemberReport")
                    };
                    return View(PathHelper.GetThemeViewPath("ReportMember"), viewModel);
                }
            }
            return ErrorToHomePage(Lang("Errors.GenericMessage"));
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
                var pagedMessages = PrivateMessageService.GetPagedReceivedMessagesByUser(pageIndex, AppConstants.PrivateMessageListSize, CurrentMember);
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
                if (string.IsNullOrEmpty(id))
                {
                    return ErrorToHomePage(Lang("Errors.GenericMessage"));
                }

                var message = PrivateMessageService.Get(new Guid(id));

                if (message.MemberToId == CurrentMember.Id | message.MemberFromId == CurrentMember.Id)
                {
                    //Mark as read if this is the receiver of the message
                    if (message.MemberToId == CurrentMember.Id)
                    {
                        // Update message as read
                        message.IsRead = true;

                        // Get the sent version and update that too
                        var sentMessage = PrivateMessageService.GetMatchingSentPrivateMessage(message.Subject, message.DateSent, message.MemberFromId, message.MemberToId);
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
                    var viewModel = new ViewPrivateMessageViewModel(page)
                    {
                        Message = message,
                        PageTitle = message.Subject
                    };
                    return View(PathHelper.GetThemeViewPath("PrivateMessageView"), viewModel);
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
                var pagedMessages = PrivateMessageService.GetPagedSentMessagesByUser(pageIndex, AppConstants.PrivateMessageListSize, CurrentMember);
                var viewModel = new PageListPrivateMessageViewModel(page)
                {
                    ListPrivateMessageViewModel = new ListPrivateMessageViewModel
                    {
                        Messages = pagedMessages
                    },
                    PageTitle = Lang("PM.SentPrivateMessages")
                };
                return View(PathHelper.GetThemeViewPath("PrivateMessagesSent"), viewModel);
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
            var lastMessage = PrivateMessageService.GetLastSentPrivateMessage(CurrentMember.Id);
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
            var senderCount = PrivateMessageService.GetAllSentByUser(CurrentMember.Id).Count;
            if (senderCount > Settings.PrivateMessageInboxSize)
            {
                ShowMessage(new GenericMessageViewModel
                {
                    Message = Lang("PM.SentItemsOverCapcity"),
                    MessageType = GenericMessages.Danger
                });
                return Redirect(Urls.GenerateUrl(Urls.UrlType.MessageInbox));
            }

            var viewModel = new PageCreatePrivateMessageViewModel(page)
            {
                CreatePrivateMessageViewModel = new CreatePrivateMessageViewModel(),
                PageTitle = Lang("PM.CreatePrivateMessage")
            };

            // add the username to the to box if available
            if (to != null)
            {
                var userTo = MemberService.Get(Convert.ToInt32(to));
                viewModel.CreatePrivateMessageViewModel.UserToUsername = userTo.UserName;
            }

            // See if this is a reply or not
            if (id != null)
            {
                var previousMessage = PrivateMessageService.Get(new Guid(id));
                // Its a reply, get the details
                viewModel.CreatePrivateMessageViewModel.UserToUsername = previousMessage.MemberFrom.UserName;
                viewModel.CreatePrivateMessageViewModel.Subject = previousMessage.Subject;
                viewModel.CreatePrivateMessageViewModel.PreviousMessage = previousMessage.Message;
            }
            return View(PathHelper.GetThemeViewPath("PrivateMessagesCreate"), viewModel);
        }

        [Authorize]
        public ActionResult EditPost(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Got to get a lot of things here as we have to check permissions
                // Get the post
                var id = Request["id"];
                if (string.IsNullOrEmpty(id))
                {
                    return ErrorToHomePage(Lang("Errors.GenericMessage"));
                }
                var post = PostService.Get(new Guid(id));

                // Get the topic
                var topic = post.Topic;
                var category = CategoryService.Get(topic.CategoryId);

                // get the users permissions
                var permissions = PermissionService.GetPermissions(category, _membersGroup, MemberService, CategoryPermissionService);

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
                        viewModel.Categories = CategoryService.GetAllowedCategories(_membersGroup, PermissionService, MemberService, CategoryPermissionService).ToList();
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
                    if (!string.IsNullOrEmpty(id))
                    {
                        var post = PostService.Get(new Guid(id));
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

                var postIds = FavouriteService.GetAllByMember(CurrentMember.Id).Select(x => x.PostId);
                var allPosts = PostService.Get(postIds.ToList());
                viewModel.Posts = allPosts;           
                return View(PathHelper.GetThemeViewPath("Favourites"), viewModel);
            }
        }

        public ActionResult Badges(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var allBadges = BadgeService.GetallBadges();

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
                var activities = ActivityService.GetPagedGroupedActivities(pageIndex, Settings.ActivitiesPerPage, BadgeService, MemberService);

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
                var cats = CategoryService.GetAll();

                // Get the latest topics
                var topics = TopicService.GetRecentRssTopics(AppConstants.ActiveTopicsListSize, cats.Select(x => x.Id).ToList());

                // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // create permissions
                var permissions = new Dictionary<Category, PermissionSet>();

                // loop through the categories and get the permissions
                foreach (var category in categories)
                {
                    var permissionSet = PermissionService.GetPermissions(category, _membersGroup, MemberService, CategoryPermissionService);
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
                    var category = CategoryService.Get(Convert.ToInt32(catId));

                    // check the user has permission to this category
                    var permissions = PermissionService.GetPermissions(category, _membersGroup, MemberService, CategoryPermissionService);

                    if (!permissions[AppConstants.PermissionDenyAccess].IsTicked)
                    {
                        var topics = TopicService.GetRssTopicsByCategory(AppConstants.ActiveTopicsListSize, category.Id);

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

                var activities = ActivityService.GetAll(20, BadgeService, MemberService).OrderByDescending(x => x.ActivityMapped.Timestamp);

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
                            RssImage = AppHelpers.ReturnBadgeUrl(badgeActivity.Badge.Image).Replace("~", ""),
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