using System.Web.Hosting;
using System.Xml;
using Umbraco.Web;

namespace Dialogue.Logic.Installation
{
    public class UnInstallHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        public static void RemoveSection()
        {
            //Get the Umbraco Service's Api's
            var services = UmbracoContext.Current.Application.Services;

            //Check to see if the section is still here (should be)
            var dialogueSection = services.SectionService.GetByAlias("dialogue");

            if (dialogueSection != null)
            {
                //Delete the section from the application
                services.SectionService.DeleteSection(dialogueSection);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveSectionLanguageKeys()
        {
            var saveFile = false;

            //Open up language file
            //umbraco/config/lang/en.xml
            const string langPath = "~/umbraco/config/lang/en.xml";

            //Path to the file resolved
            var langFilePath = HostingEnvironment.MapPath(langPath);

            //Load settings.config XML file
            var langXml = new XmlDocument();
            langXml.Load(langFilePath);

            // Section Node
            // <area alias="sections">
            var sectionNode = langXml.SelectSingleNode("//area [@alias='sections']");

            if (sectionNode != null)
            {
                var findSectionKey = sectionNode.SelectSingleNode("./key [@alias='dialogue']");

                if (findSectionKey != null)
                {
                    //Let's remove the key from XML...
                    sectionNode.RemoveChild(findSectionKey);

                    //Save the file flag to true
                    saveFile = true;
                }
            }

            // Section Node
            // <area alias="treeHeaders">
            var treeNode = langXml.SelectSingleNode("//area [@alias='treeHeaders']");

            if (treeNode != null)
            {
                var findTreeKey = treeNode.SelectSingleNode("./key [@alias='dialogue']");

                if (findTreeKey != null)
                {
                    //Let's remove the key from XML...
                    treeNode.RemoveChild(findTreeKey);

                    //Save the file flag to true
                    saveFile = true;
                }
            }

            //If saveFile flag is true then save the file
            if (saveFile)
            {
                //Save the XML file
                langXml.Save(langFilePath);
            }


        }
    }
}