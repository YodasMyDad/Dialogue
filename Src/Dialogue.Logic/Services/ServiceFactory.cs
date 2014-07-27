using System.Web;

namespace Dialogue.Logic.Services
{
    public static class ServiceFactory
    {
        //TODO - Create per request services here and use throughout the site
        
        public static ActivityService ActivityService
        {
            get
            {
                const string activityKey = "ActivityService";
                if (!HttpContext.Current.Items.Contains(activityKey))
                {
                    HttpContext.Current.Items.Add(activityKey, new ActivityService());
                }
                return HttpContext.Current.Items[activityKey] as ActivityService;
            }
        }

    }
}