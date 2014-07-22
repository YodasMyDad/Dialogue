using System.Linq;
using System.Web.Hosting;
using System.Xml;
using Umbraco.Core;

namespace Dialogue.Logic.Installation
{
    public class InstallHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationContext"></param>
        public static void AddSection(ApplicationContext applicationContext)
        {
            //Get SectionService
            var sectionService = applicationContext.Services.SectionService;

            //Try & find a section with the alias of "dialogueSection"
            var dialogueSection = sectionService.GetSections().SingleOrDefault(x => x.Alias == "dialogue");

            //If we can't find the section - doesn't exist
            if (dialogueSection == null)
            {
                //So let's create it the section
                sectionService.MakeNew("dialogue", "dialogue", "icon-chat");

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddSectionLanguageKeys()
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

                if (findSectionKey == null)
                {
                    //Let's add the key
                    var attrToAdd = langXml.CreateAttribute("alias");
                    attrToAdd.Value = "dialogue";

                    var keyToAdd = langXml.CreateElement("key");
                    keyToAdd.InnerText = "Dialogue";
                    keyToAdd.Attributes.Append(attrToAdd);

                    sectionNode.AppendChild(keyToAdd);

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

                if (findTreeKey == null)
                {
                    //Let's add the key
                    var attrToAdd = langXml.CreateAttribute("alias");
                    attrToAdd.Value = "dialogue";

                    var keyToAdd = langXml.CreateElement("key");
                    keyToAdd.InnerText = "Dialogue";
                    keyToAdd.Attributes.Append(attrToAdd);

                    treeNode.AppendChild(keyToAdd);

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

//        /// <summary>
//        /// 
//        /// </summary>
//        public static void AddSectionDashboard()
//        {
//            var saveFile = false;

//            //Open up language file
//            //umbraco/config/lang/en.xml
//            const string dashboardPath = "~/config/dashboard.config";

//            //Path to the file resolved
//            var dashboardFilePath = HostingEnvironment.MapPath(dashboardPath);

//            //Load settings.config XML file
//            var dashboardXml = new XmlDocument();
//            dashboardXml.Load(dashboardFilePath);

//            // Section Node
//            var findSection = dashboardXml.SelectSingleNode("//section [@alias='dialogueDashboardSection']");

//            //Couldn't find it
//            if (findSection == null)
//            {
//                //Let's add the xml
//                const string xmlToAdd = @"<section alias='dialogueDashboardSection'>
//                                            <areas>
//                                                <area>dialogue</area>
//                                            </areas>
//                                            <tab caption='Last 7 days'>
//                                                <control addPanel='true' panelCaption=''>/App_Plugins/dialogue/backOffice/dialogueTree/partials/dashboard.html</control>
//                                            </tab>
//                                        </section>";

//                //Get the main root <dashboard> node
//                var dashboardNode = dashboardXml.SelectSingleNode("//dashBoard");

//                if (dashboardNode != null)
//                {
//                    //Load in the XML string above
//                    var xmlNodeToAdd = new XmlDocument();
//                    xmlNodeToAdd.LoadXml(xmlToAdd);

//                    //Append the xml above to the dashboard node
//                    dashboardNode.AppendChild(xmlNodeToAdd);

//                    //Save the file flag to true
//                    saveFile = true;
//                }
//            }

//            //If saveFile flag is true then save the file
//            if (saveFile)
//            {
//                //Save the XML file
//                dashboardXml.Save(dashboardFilePath);
//            }
//        }
    }
}