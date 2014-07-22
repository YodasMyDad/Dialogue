using System.Web.Mvc;
using System.Web.Routing;

namespace Dialogue.Logic.Routes
{
    public static class RouteCollectionExtensions
    {
        public static Route MapUmbracoRoute(this RouteCollection routes,
                                            string name, string url, object defaults, UmbracoVirtualNodeRouteHandler virtualNodeHandler,
                                            object constraints = null, string[] namespaces = null)
        {
            //HACK
            url = url.Replace("//", "/");
            var route = RouteTable.Routes.MapRoute(name, url, defaults, constraints, namespaces);
            route.RouteHandler = virtualNodeHandler;
            return route;
        }
    }
}