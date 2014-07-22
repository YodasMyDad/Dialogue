using System.Web;

namespace Dialogue.Logic.Data.Context
{
    public static class ContextPerRequest
    {
        public const string MyDbPerRequestContext = "Dialogue-Context-PR";

        public static DatabaseContext Db
        {
            get
            {
                if (!HttpContext.Current.Items.Contains(MyDbPerRequestContext))
                {
                    HttpContext.Current.Items.Add(MyDbPerRequestContext, new DatabaseContext());
                }

                return HttpContext.Current.Items[MyDbPerRequestContext] as DatabaseContext;
            }
        }
    }
}