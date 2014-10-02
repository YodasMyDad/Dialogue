using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;

namespace Dialogue.Logic.Routes
{
    /// <summary>
    /// Lifted from The Shaz'master frm his Articulate for Umbraco package
    /// </summary>
    public abstract class UmbracoVirtualNodeRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var umbracoContext = UmbracoContext.Current;

            //TODO: Problem with the core and EnsurePublishedContentRequestAttribute - If more than one language then it just chooses default
            //TODO: Currently speaking to Shannon about this
            var defaultLanguage = umbraco.cms.businesslogic.language.Language.GetAllAsList().FirstOrDefault();
            var currentCulture = ((defaultLanguage == null) ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.CultureAlias));
         
            // Pass the 
            var ensurePcr = new EnsurePublishedContentRequestAttribute(umbracoContext, "__virtualnodefinder__", currentCulture.Name);

            var found = FindContent(requestContext, umbracoContext);
            if (found == null) return new NotFoundHandler();

            //assign the node to our special token
            requestContext.RouteData.DataTokens["__virtualnodefinder__"] = found;

            //this hack creates and assigns the pcr to the context
            ensurePcr.OnActionExecuted(new ActionExecutedContext { RequestContext = requestContext });

            //allows inheritors to change the pcr
            PreparePublishedContentRequest(umbracoContext.PublishedContentRequest);

            //create the render model
            var renderModel = new RenderModel(umbracoContext.PublishedContentRequest.PublishedContent, umbracoContext.PublishedContentRequest.Culture);

            //assigns the required tokens to the request
            requestContext.RouteData.DataTokens.Add("umbraco", renderModel);
            requestContext.RouteData.DataTokens.Add("umbraco-doc-request", umbracoContext.PublishedContentRequest);
            requestContext.RouteData.DataTokens.Add("umbraco-context", umbracoContext);

            umbracoContext.PublishedContentRequest.ConfigureRequest();

            return new MvcHandler(requestContext);
        }

        protected abstract IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext);

        protected virtual void PreparePublishedContentRequest(PublishedContentRequest publishedContentRequest)
        {
        }
    }
}