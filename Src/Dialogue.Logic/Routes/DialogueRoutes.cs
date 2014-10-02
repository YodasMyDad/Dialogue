using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Dialogue.Logic.Constants;
using Umbraco.Core;
using Umbraco.Core.Models;
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
                // For each articulate root, we need to create some custom route, BUT routes can overlap
                // based on multi-tenency so we need to deal with that. 
                // For example a root articulate node might yield a route like:
                //      /
                // and another articulate root node that has a domain might have this url:
                //      http://mydomain/
                // but when that is processed through RoutePathFromNodeUrl, it becomes:
                //      /
                // which already exists and is already assigned to a specific node ID.
                // So what we need to do in these cases is use a special route handler that takes
                // into account the domain assigned to the route.
                var groups = dialogueNodes.GroupBy(x => RouteCollectionExtensions.RoutePathFromNodeUrl(x.Url));
                foreach (var grouping in groups)
                {
                    var groupHash = grouping.Key.GetHashCode();

                    RemoveExisting(routes,
                        string.Format(TopicRouteName, groupHash),
                        string.Format(MemberRouteName, groupHash),
                        string.Format(DialoguePageRouteName, groupHash)
                    );

                    var nodesAsArray = grouping.ToArray();

                    MapTopicRoute(routes, grouping.Key, nodesAsArray);
                    MapDialoguePages(routes, grouping.Key, nodesAsArray);
                    MapMemberRoute(routes, grouping.Key, nodesAsArray);
                }
            }

        }

        /// <summary>
        /// Create the member profile route - It's a fake page
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="nodeRoutePath"></param>
        /// <param name="nodesWithPath"></param>
        private static void MapMemberRoute(RouteCollection routes, string nodeRoutePath, IPublishedContent[] nodesWithPath)
        {
 
            foreach (var nodeSearch in nodesWithPath.GroupBy(x => x.GetPropertyValue<string>(AppConstants.PropMemberUrlName)))
            {
                var routeHash = nodeSearch.Key.GetHashCode();

                //Create the route for the /search/{term} results
                routes.MapUmbracoRoute(
                    string.Format(MemberRouteName, routeHash),
                    (nodeRoutePath.EnsureEndsWith('/') + nodeSearch.Key + "/{membername}").TrimStart('/'),
                    new
                    {
                        controller = "DialogueMember",
                        action = "Show",
                        topicname = UrlParameter.Optional
                    },
                    new DialogueMemberRouteHandler(nodesWithPath));
            }

        }

        /// <summary>
        /// Create the Topic route - It's a fake page
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="nodeRoutePath"></param>
        /// <param name="nodesWithPath"></param>
        private static void MapTopicRoute(RouteCollection routes, string nodeRoutePath, IPublishedContent[] nodesWithPath)
        {
            foreach (var nodeSearch in nodesWithPath.GroupBy(x => x.GetPropertyValue<string>(AppConstants.PropTopicUrlName)))
            {
                var routeHash = nodeSearch.Key.GetHashCode();

                //Create the route for the /search/{term} results
                routes.MapUmbracoRoute(
                    string.Format(TopicRouteName, routeHash),
                    (nodeRoutePath.EnsureEndsWith('/') + nodeSearch.Key + "/{topicname}").TrimStart('/'),
                    new
                    {
                        controller = "DialogueTopic",
                        action = "Show",
                        topicname = UrlParameter.Optional
                    },
                    new DialogueTopicRouteHandler(nodesWithPath));
            }
        }

        /// <summary>
        /// Create the dialogue page route - It's a fake page
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="nodeRoutePath"></param>
        /// <param name="nodesWithPath"></param>
        private static void MapDialoguePages(RouteCollection routes, string nodeRoutePath, IPublishedContent[] nodesWithPath)
        {

            foreach (var nodeSearch in nodesWithPath.GroupBy(x => x.GetPropertyValue<string>(AppConstants.PropDialogueUrlName)))
            {
                var routeHash = nodeSearch.Key.GetHashCode();

                routes.MapUmbracoRoute(
                    string.Format(DialoguePageRouteName, routeHash),
                    (nodeRoutePath.EnsureEndsWith('/') + nodeSearch.Key + "/{pagename}").TrimStart('/'),
                    new
                    {
                        controller = "DialoguePage",
                        action = "Show",
                        topicname = UrlParameter.Optional
                    },
                    new DialoguePageRouteHandler(nodesWithPath));
            }
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