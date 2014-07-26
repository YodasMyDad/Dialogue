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
    public class DialoguePageController : BaseController
    {
        private readonly CategoryService _categoryService;
        private readonly IMemberGroup _membersGroup;
        private readonly TopicService _topicService;
        private readonly BadgeService _badgeService;
        private readonly ActivityService _activityService;

        public DialoguePageController()
        {
            _categoryService = new CategoryService();
            _topicService = new TopicService();
            _badgeService = new BadgeService();
            _activityService = new ActivityService();
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

            // Show Badges
            if (pagename.ToLower().Contains(AppConstants.PageUrlBadges))
            {
                return Badges(page);
            }

            // Show Activities
            if (pagename.ToLower().Contains(AppConstants.PageUrlActivity))
            {
                return Activity(page);
            }
            
            // We return null here as this actionresult is purely used
            // to display virtual dialogue pages
            return null;
        }


        public ActionResult Badges(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var allBadges = _badgeService.GetallBadges();

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
                var activities = _activityService.GetPagedGroupedActivities(pageIndex, Settings.ActivitiesPerPage);

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

                //TODO - Get Topics for only the current forum
                // Get the latest topics
                var topics = _topicService.GetRecentRssTopics(AppConstants.ActiveTopicsListSize);

                // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // create permissions
                var permissions = new Dictionary<Category, PermissionSet>();

                // loop through the categories and get the permissions
                foreach (var category in categories)
                {
                    var permissionSet = PermissionService.GetPermissions(category, _membersGroup);
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
                                rssTopics.Add(new RssItem { Description = firstOrDefault.PostContent, Link = topic.NiceUrl, Title = topic.Name, PublishedDate = topic.CreateDate });
                        }
                    }
                }

                return new RssResult(rssTopics, Lang("Rss.LatestTopics.Title"), Lang("Rss.LatestTopics.Description"));
            }

        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public ActionResult ActivityRss(DialoguePage page)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // get an rss lit ready
                var rssActivities = new List<RssItem>();

                var activities = _activityService.GetAll(20).OrderByDescending(x => x.ActivityMapped.Timestamp);

                var activityLink = UrlTypes.GenerateUrl(UrlTypes.UrlType.Activity);

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