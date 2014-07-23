using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Dialogue.Logic.Constants;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.presentation.umbraco;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Dialogue.Logic.Routes
{
    public static class DialogueRoutes
    {
        public const string TopicRouteName = "dialogue_topic_{0}";
        public const string MemberRouteName = "dialogue_member_{0}";
        public const string DialoguePageRouteName = "dialogue_page_{0}";

        public static void MapRoutes(RouteCollection routes, ContextualPublishedCache umbracoCache)
        {
            //find all Dialogue forum root nodes
            var dialogueNodes = umbracoCache.GetByXPath(string.Concat("//", AppConstants.DocTypeForumRoot)).ToArray();
           
            //NOTE: need to write lock because this might need to be remapped while the app is running if
            // any articulate nodes are updated with new values
            using (routes.GetWriteLock())
            {
                //for each one of them we need to create some virtual routes/nodes
                foreach (var node in dialogueNodes)
                {
                    RemoveExisting(routes,
                        string.Format(TopicRouteName, node.Id),
                        string.Format(MemberRouteName, node.Id),
                        string.Format(DialoguePageRouteName, node.Id)
                        );

                    MapTopicRoute(routes, node);
                    MapDialoguePages(routes, node);
                    MapMemberRoute(routes, node);
                }
            }
        }

        /// <summary>
        /// Create the Topic route - It's a fake page
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="node"></param>
        private static void MapMemberRoute(RouteCollection routes, IPublishedContent node)
        {
            //Create the route for the /search/{term} results
            routes.MapUmbracoRoute(
                string.Format(MemberRouteName, node.Id),
                (node.Url.EnsureEndsWith('/') + node.GetPropertyValue<string>(AppConstants.PropMemberUrlName) + "/{membername}").TrimStart('/'),
                new
                {
                    controller = "DialogueMember",
                    action = "Show",
                    topicname = UrlParameter.Optional
                },
                new PageBySlugRouteHandler(node.Id, node.GetPropertyValue<string>(AppConstants.PropMemberUrlName), "MemberPageNamePlaceHolder"));
        }

        /// <summary>
        /// Create the Topic route - It's a fake page
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="node"></param>
        private static void MapTopicRoute(RouteCollection routes, IPublishedContent node)
        {
            //Create the route for the /search/{term} results
            routes.MapUmbracoRoute(
                string.Format(TopicRouteName, node.Id),
                (node.Url.EnsureEndsWith('/') + node.GetPropertyValue<string>(AppConstants.PropTopicUrlName) + "/{topicname}").TrimStart('/'),
                new
                {
                    controller = "DialogueTopic",
                    action = "Show",
                    topicname = UrlParameter.Optional
                },
                new PageBySlugRouteHandler(node.Id, node.GetPropertyValue<string>(AppConstants.PropTopicUrlName), "TopicPageNamePlaceHolder"));
        }

        /// <summary>
        /// Create the Topic route - It's a fake page
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="node"></param>
        private static void MapDialoguePages(RouteCollection routes, IPublishedContent node)
        {
            //Create the route for the /search/{term} results
            routes.MapUmbracoRoute(
                string.Format(DialoguePageRouteName, node.Id),
                (node.Url.EnsureEndsWith('/') + node.GetPropertyValue<string>(AppConstants.PropDialogueUrlName) + "/{pagename}").TrimStart('/'),
                new
                {
                    controller = "DialoguePage",
                    action = "Show",
                    pagename = UrlParameter.Optional
                },
                new PageBySlugRouteHandler(node.Id, node.GetPropertyValue<string>(AppConstants.PropDialogueUrlName), "DialoguePageNamePlaceHolder"));
        }

        private static void RemoveExisting(RouteCollection routes, params string[] names)
        {
            foreach (var name in names)
            {
                var r = routes[name];
                if (r != null)
                {
                    routes.Remove(r);
                }
            }
        }
    }
}