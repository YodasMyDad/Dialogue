using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Dialogue.Logic.Routes
{
    public class PageBySlugRouteHandler : UmbracoVirtualNodeByIdRouteHandler
    {
        private readonly string _urlName;
        private readonly string _pageName;

        public PageBySlugRouteHandler(int realNodeId,
            string topicUrlName,
            string topicPageName)
            : base(realNodeId)
        {
            _urlName = topicUrlName;
            _pageName = topicPageName;
        }

        protected override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext, IPublishedContent baseContent)
        {
            var controllerName = requestContext.RouteData.GetRequiredString("controller");
            var rootUrl = baseContent.Url;

            return new DialogueVirtualPage(
                baseContent,
                _pageName,
                controllerName,
                rootUrl.EnsureEndsWith('/') + _urlName);
        }
    }
}