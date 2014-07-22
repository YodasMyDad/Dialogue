using System.Web;

namespace Dialogue.Logic
{
    public class NotFoundHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.StatusCode = 404;
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}