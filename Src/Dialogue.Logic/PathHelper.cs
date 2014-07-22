using Dialogue.Logic.Application;
using Dialogue.Logic.Models;

namespace Dialogue.Logic
{
    public static class PathHelper
    {
        public static string GetThemePath()
        {
            const string path = "~/App_Plugins/Dialogue/Themes/{0}/";
            return string.Format(path, Dialogue.Settings().Theme);
        }

        public static string GetThemeViewPath(string viewName)
        {
            const string path = "~/App_Plugins/Dialogue/Themes/{0}/Views/{1}.cshtml";
            return string.Format(path, Dialogue.Settings().Theme, viewName);
        }

        public static string GetThemePartialViewPath(string viewName)
        {
            const string path = "~/App_Plugins/Dialogue/Themes/{0}/Views/Shared/{1}.cshtml";
            return string.Format(path, Dialogue.Settings().Theme, viewName);
        }
    }
}