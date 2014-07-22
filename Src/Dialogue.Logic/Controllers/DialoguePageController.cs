using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
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

        public DialoguePageController()
        {
            _categoryService = new CategoryService();
            _topicService = new TopicService();
            _membersGroup = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        /// <summary>
        /// Used to render virtual dialogue pages
        /// </summary>
        public ActionResult Show(RenderModel model, string pagename, int? p = null)
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
            
            // We return null here as this actionresult is purely used
            // to display virtual dialogue pages
            return null;
        }

        public ActionResult Leaderboard(DialoguePage page)
        {
            page.PageTitle = Lang("Page.Leaderboard.PageTitle");
 
           return View(PathHelper.GetThemeViewPath("Leaderboard"), page);

        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public ActionResult TopicsRss(DialoguePage page)
        {
            //page.PageTitle = Lang("Page.TopicsRss.PageTitle");

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // get an rss lit ready
                var rssTopics = new List<RssItem>();

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

                return new RssResult(rssTopics, Lang("Rss.LatestActivity.Title"), Lang("Rss.LatestActivity.Description"));
            }

        }
    }
}