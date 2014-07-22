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
    public class PartialPickerApiController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<PartialPicker> GetFiles(string folder)
        {
            var list = new List<PartialPicker>();
            if (!string.IsNullOrEmpty(folder))
            {
                var files = Directory.GetFiles(HostingEnvironment.MapPath(folder), "*.cshtml").Select(Path.GetFileName).ToArray();
                list.AddRange(files.Select(file => new PartialPicker {File = file}));
            }
            return list;
        } 
    }
}