namespace Dialogue.Logic.Application
{
    using System.IO;
    using System.Web.Hosting;
    using System.Web.Security;
    using Umbraco.Core;
    using Umbraco.Core.Configuration;
    using Umbraco.Web;
    using Umbraco.Web.Routing;
    using Umbraco.Web.Security;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Web;

    public static class ContextHelper
    {
        // Hold on to your hats. Hack of 2017 starts here baby!
        /// <summary>
        /// Some Ajax requests have the wrong culture so the language strings are incorrect, this corrects it (Don't judge me, you know you've done worse)
        /// </summary>
        public static void EnsureCorrectCulture()
        {
            const string key = "umb-culture-correction";
            if (!HttpContext.Current.Items.Contains(key))
            {
                var helper = AppHelpers.UmbHelper();

                // We get the first one, as Dialogue is really only built for a single forum root
                var homeNode = helper.TypedContentAtXPath("//root//Dialogue").FirstOrDefault();
                var correctCulture = homeNode.GetCulture();
                Thread.CurrentThread.CurrentCulture = correctCulture;
                Thread.CurrentThread.CurrentUICulture = correctCulture;

                // We just add true so we know it's been done
                HttpContext.Current.Items.Add(key, true);
            }
        }

        public static UmbracoContext EnsureUmbracoContext()
        {
            //TODO: To get at the IPublishedCaches it is only available on the UmbracoContext (which we need to fix)
            // but since this method operates async, there isn't one, so we need to make our own to get at the cache
            // object by creating a fake HttpContext. Not pretty but it works for now.
            if (UmbracoContext.Current == null)
            {
                var dummyHttpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("blah.aspx", "", new StringWriter())));
                return UmbracoContext.EnsureContext(
                    dummyHttpContext,
                    ApplicationContext.Current,
                    new WebSecurity(dummyHttpContext, ApplicationContext.Current),
                    UmbracoConfig.For.UmbracoSettings(),
                    UrlProviderResolver.Current.Providers,
                    false);
            }

            return UmbracoContext.Current;
        }

        public static void EnsurePublishedContent(int nodeId)
        {
            //UmbracoContext.Current.PublishedContentRequest = new PublishedContentRequest();
            //UmbracoContext.Current.PublishedContentRequest.PublishedContent = null;
            UmbracoContext currentContext;
            if (UmbracoContext.Current == null)
            {
                currentContext = EnsureUmbracoContext();
            }
            else
            {
                currentContext = UmbracoContext.Current;
            }

            if (HttpContext.Current == null)
            {
                SetFakeHttpContext();
            }

            if (currentContext.PublishedContentRequest == null)
            {
                var requestUrl = new Uri("http://localhost");
                var request = HttpContext.Current.Request;
                if (request != null)
                {
                    requestUrl = request.Url;
                }
                var uri = UriUtility.UriToUmbraco(requestUrl);
                UmbracoContext.Current.PublishedContentRequest = new PublishedContentRequest(uri, currentContext.RoutingContext, UmbracoConfig.For.UmbracoSettings().WebRouting, s => Roles.Provider.GetRolesForUser(s))
                {
                    PublishedContent = new UmbracoHelper(currentContext).TypedContent(nodeId)
                };
            }
        }

        public static void SetFakeHttpContext()
        {
            HttpContext.Current = new HttpContext(new SimpleWorkerRequest("blah.aspx", "", new StringWriter()));
        }
    }
}