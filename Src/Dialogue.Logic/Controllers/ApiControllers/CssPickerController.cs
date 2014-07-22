using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Dialogue.Logic.Models.Plugins;
using Umbraco.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.WebApi;

namespace Dialogue.Logic.Controllers.PluginControllers
{
    [PluginController("Apt")]
    [IsBackOffice]
    public class CssPickerApiController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<CssFile> GetFiles(string folder)
        {
            var list = new List<CssFile>();
            if (!string.IsNullOrEmpty(folder))
            {
                var files = Directory.GetFiles(HostingEnvironment.MapPath(folder), "*.css").Select(Path.GetFileName).ToArray();
                list.AddRange(files.Select(file => new CssFile { Name = file }));
            }
            return list;
        } 
    }
}