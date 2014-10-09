using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.UI;
using System.Xml;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.UserControls
{
    public partial class DialogueInstaller : UserControl
    {
        public InstallerResult InstallerResult { get; set; }
        private const string PFormat = "<p>{0}</p>";
        private const string HFormat = "<h5>{0}</h5>";

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (Page.IsPostBack && InstallerResult != null && InstallerResult.ResultItems.Any())
            {
                // Show Results
                if (InstallerResult.CompletedSuccessfully())
                {
                    InstallerSuccessfull.Visible = true;
                    pnlMainText.Visible = false;
                }
                InstallerResultPanel.Visible = true;
                litResults.Text = FormatInstallerResults(InstallerResult);
            }
            else if (Page.IsPostBack)
            {
                InstallerResultPanel.Visible = true;
                litResults.Text = @"Installer results are empty? Try the installer again. 
                                    If the same thing happens, a manual check will be needed to make sure everything has installed correctly.";
            }
        }

        private string FormatInstallerResults(InstallerResult installerResult)
        {

            var sb = new StringBuilder();

            // Loop through results and display
            foreach (var resultItem in installerResult.ResultItems)
            {
                CreateResultEntry(resultItem, sb);
            }

            return sb.ToString();
        }

        private void CreateResultEntry(ResultItem resultItem, StringBuilder sb)
        {
            sb.Append("<div style='margin:10px 0;padding:10px 0;border-bottom:1px #efefef dotted'>").AppendLine();
            sb.AppendFormat(HFormat, resultItem.Name).AppendLine();
            sb.AppendFormat(PFormat, resultItem.Description).AppendLine();
            if (!resultItem.CompletedSuccessfully)
            {
                sb.AppendFormat(PFormat, string.Concat("Completed Successfully: ", resultItem.CompletedSuccessfully)).AppendLine();   
            }
            sb.Append("<div>").AppendLine();
        }

        protected void CompleteInstallation(object sender, EventArgs e)
        {
            InstallerResult = new InstallerResult();

            try
            {

                // Check member type exists
                var memberTypeResult = new ResultItem
                {
                    CompletedSuccessfully = false,
                    Name = string.Format("Creating the Member Type {0}", AppConstants.MemberTypeAlias)
                };
                var typeExists = ServiceFactory.MemberService.MemberTypeExists(AppConstants.MemberTypeAlias);
                if (!typeExists)
                {
                    // create the Dialogue membertype
                    ServiceFactory.MemberService.AddDialogueMemberType();
                    memberTypeResult.Description = "Done successfully";
                    memberTypeResult.CompletedSuccessfully = true;
                }
                else
                {
                    memberTypeResult.Description = "Skipped as member type already exists";
                    memberTypeResult.CompletedSuccessfully = true;
                }
                InstallerResult.ResultItems.Add(memberTypeResult);


                // Add Member Groups (Admin, Guest, Standard)
                var adminRoleResult = new ResultItem
                {
                    CompletedSuccessfully = false,
                    Name = string.Format("Creating the role: {0}", AppConstants.AdminRoleName)
                };
                var adminExists = ServiceFactory.MemberService.MemberGroupExists(AppConstants.AdminRoleName);
                if (!adminExists)
                {
                    // Create it
                    ServiceFactory.MemberService.CreateMemberGroup(AppConstants.AdminRoleName);
                    adminRoleResult.Description = "Done successfully";
                    adminRoleResult.CompletedSuccessfully = true;
                }
                else
                {
                    adminRoleResult.Description = "Skipped as role already exists";
                    adminRoleResult.CompletedSuccessfully = true;
                }
                InstallerResult.ResultItems.Add(adminRoleResult);


                var guestRoleResult = new ResultItem
                {
                    CompletedSuccessfully = false,
                    Name = string.Format("Creating the role: {0}", AppConstants.GuestRoleName)
                };
                var guestExists = ServiceFactory.MemberService.MemberGroupExists(AppConstants.GuestRoleName);
                if (!guestExists)
                {
                    // Create it
                    ServiceFactory.MemberService.CreateMemberGroup(AppConstants.GuestRoleName);
                    guestRoleResult.Description = "Done successfully";
                    guestRoleResult.CompletedSuccessfully = true;
                }
                else
                {
                    guestRoleResult.Description = "Skipped as role already exists";
                    guestRoleResult.CompletedSuccessfully = true;
                }
                InstallerResult.ResultItems.Add(guestRoleResult);


                var standardRoleResult = new ResultItem
                {
                    CompletedSuccessfully = false,
                    Name = string.Format("Creating the role: {0}", AppConstants.MemberGroupDefault)
                };
                var standardExists = ServiceFactory.MemberService.MemberGroupExists(AppConstants.MemberGroupDefault);
                if (!standardExists)
                {
                    // Create it
                    ServiceFactory.MemberService.CreateMemberGroup(AppConstants.MemberGroupDefault);
                    standardRoleResult.Description = "Done successfully";
                    standardRoleResult.CompletedSuccessfully = true;
                }
                else
                {
                    standardRoleResult.Description = "Skipped as role already exists";
                    standardRoleResult.CompletedSuccessfully = true;
                }
                InstallerResult.ResultItems.Add(standardRoleResult);

                // ############ Web.Config Stuff
                var webConfigPath = HostingEnvironment.MapPath("~/web.config");
                if (webConfigPath != null)
                {
                    var xDoc = new XmlDocument();
                    xDoc.Load(webConfigPath);

                    // Entity Framework Configuration Sections
                    var efResult = new ResultItem
                    {
                        CompletedSuccessfully = false,
                        Name = "Add Entity Framework Config Sections"
                    };
                    if (!IsEntityFrameworkAlreadyInstalled(xDoc))
                    {
                        // TODO - Last as it will recycle app pool
                        // Add Entity Framework Entries into Web.config
                        AddEntityFrameworkConfigSections(xDoc);
                        efResult.CompletedSuccessfully = true;
                        efResult.Description = "Successfully added the config sections to the web.config";

                        // Tell the installer to save the config
                        efResult.RequiresConfigUpdate = true;
                    }
                    else
                    {
                        efResult.CompletedSuccessfully = true;
                        efResult.Description = "Entity Framework already installed, so skipped.";
                    }
                    InstallerResult.ResultItems.Add(efResult);

                    // Add required appsettings
                    InstallerResult.ResultItems.Add(AddAppSetting("ClientValidationEnabled", "true", xDoc));
                    InstallerResult.ResultItems.Add(AddAppSetting("UnobtrusiveJavaScriptEnabled", "true", xDoc));

                    //TODO Add other web.config changes here if needed

                    //See if any results, require a config update.
                    var updateResults = InstallerResult.ResultItems.Where(x => x.RequiresConfigUpdate);
                    if (updateResults.Any())
                    {
                        // Finally save web.config
                        xDoc.Save(webConfigPath);
                    }

                }
                else
                {
                    var nowebConfig = new ResultItem
                    {
                        CompletedSuccessfully = false,
                        Name = "No Web.Config?",
                        Description = "Installer is unable to locate the web.config using 'HostingEnvironment.MapPath(\"~/web.config\")'? Weird hey."
                    };
                    InstallerResult.ResultItems.Add(nowebConfig);
                }

                
                // ############ Dashboard Stuff
                var dashboardPath = HostingEnvironment.MapPath("~/config/dashboard.config");
                if (dashboardPath != null)
                {
                    var updateDashboardConfig = false;
                    var xDoc = new XmlDocument();
                    xDoc.Load(dashboardPath);

                    //############# Custom Dashboards

                    // Dialogue Importer Dashboard
                    var efResult = new ResultItem
                    {
                        CompletedSuccessfully = false,
                        Name = "Add Dialogue Importer Dashboard"
                    };

                    var importerUserControls = new List<string>
                    {
                        "/usercontrols/dialogueimporter.ascx"
                    };
                    efResult = AddDashboard(importerUserControls, "StartupDeveloperDashboardSection", "Dialogue Importer", xDoc);
                    InstallerResult.ResultItems.Add(efResult);
                    updateDashboardConfig = efResult.RequiresConfigUpdate;


                    // Add more dashboards here


                    // Save if update needed
                    if (updateDashboardConfig)
                    {
                        xDoc.Save(dashboardPath);
                    }

                }


            }
            catch (Exception ex)
            {
                var memberTypeResult = new ResultItem
                {
                    CompletedSuccessfully = false,
                    Name = "There was an error trying to installer",
                    Description = string.Concat(ex.Message, "<br/><br/>", ex.InnerException.Message)
                };
                InstallerResult.ResultItems.Add(memberTypeResult);
            }

        }

        private static bool IsEntityFrameworkAlreadyInstalled(XmlDocument webconfig)
        {
            var entityFrameworkConfig = webconfig.SelectSingleNode("configuration/configSections/section[@name='entityFramework']");
            return entityFrameworkConfig != null;
        }

        private static bool AppSettingsExists(XmlDocument webconfig, string key)
        {
            var appSettingsClientVal = webconfig.SelectSingleNode(string.Format("configuration/appSettings/add[@key='{0}']", key));
            return appSettingsClientVal != null;
        }

        private static bool DashboardExists(XmlDocument dbconfig, string tabCaption)
        {
            var tab = dbconfig.SelectSingleNode(string.Format("dashBoard/section/tab[@caption='{0}']", tabCaption));
            return tab != null;
        }

        /// <summary>
        /// Adds a dashboard to the config
        /// </summary>
        /// <param name="usercontrols"></param>
        /// <param name="section"></param>
        /// <param name="tabCaption"></param>
        /// <param name="dbconfig"></param>
        /// <returns></returns>
        private ResultItem AddDashboard(List<string> usercontrols, string section, string tabCaption, XmlDocument dbconfig)
        {
            var rs = new ResultItem
            {
                Name = "Adding Dashboard",
                CompletedSuccessfully = true,
                Description = string.Format("Successfully added {0} Dashboard", tabCaption),
                RequiresConfigUpdate = true
            };

            if (!DashboardExists(dbconfig, tabCaption))
            {
                try
                {
                    //StartupDeveloperDashboardSection

                    //  <tab caption="Dialogue Importer">
                    //      <control addPanel="true" panelCaption="">
                    //          /usercontrols/dialogueimporter.ascx
                    //      </control>
                    //  </tab>

                    // App settings root
                    var dbSection = dbconfig.SelectSingleNode(string.Format("dashBoard/section[@alias='{0}']", section));

                    // Create new tab
                    var tab = dbconfig.CreateNode(XmlNodeType.Element, "tab", null);
                    var captionAttr = dbconfig.CreateAttribute("caption");
                    captionAttr.Value = tabCaption;
                    tab.Attributes.Append(captionAttr);

                    // Loop through usercontrols to add controls
                    for (var i = 0; i < usercontrols.Count; i++)
                    {
                        // Create control
                        var control = dbconfig.CreateNode(XmlNodeType.Element, "control", null);
                        var addPanelAttr = dbconfig.CreateAttribute("addPanel");
                        addPanelAttr.Value = "true";
                        control.Attributes.Append(addPanelAttr);
                        control.InnerText = usercontrols[i];

                        tab.AppendChild(control);
                    }
                  
                    // Append tab to Section
                    dbSection.AppendChild(tab);

                    return rs;
                }
                catch (Exception ex)
                {
                    rs.Description = string.Format("Failed to add {0} to dashboard config, error: {1}", tabCaption, ex.InnerException);
                    rs.CompletedSuccessfully = false;
                    rs.RequiresConfigUpdate = false;
                }
            }
            else
            {
                rs.Description = string.Format("Skipped adding {0} to dashboard config, already exists", tabCaption);
                rs.CompletedSuccessfully = true;
                rs.RequiresConfigUpdate = false;
            }
            return rs;
        }

        private ResultItem AddAppSetting(string key, string value, XmlDocument webconfig)
        {
            var rs = new ResultItem
            {
                Name = "Adding AppSetting",
                CompletedSuccessfully = true,
                Description = string.Format("Successfully added {0} appsetting", key),
                RequiresConfigUpdate = true
            };

            if (!AppSettingsExists(webconfig, key))
            {
                try
                {
                    // App settings root
                    var appSettings = webconfig.SelectSingleNode("configuration/appSettings");

                    // Create new section
                    var newAppSetting = webconfig.CreateNode(XmlNodeType.Element, "add", null);

                    // Attributes
                    var keyAttr = webconfig.CreateAttribute("key");
                    keyAttr.Value = key;

                    var valueAttr = webconfig.CreateAttribute("value");
                    valueAttr.Value = value;

                    newAppSetting.Attributes.Append(keyAttr);
                    newAppSetting.Attributes.Append(valueAttr);

                    // Append it
                    appSettings.AppendChild(newAppSetting);

                    return rs;
                }
                catch (Exception ex)
                {
                    rs.Description = string.Format("Failed to add {0} to config, error: {1}", key, ex.InnerException);
                    rs.CompletedSuccessfully = false;
                    rs.RequiresConfigUpdate = false;
                }
            }
            else
            {
                rs.Description = string.Format("Skipped adding {0} to config, already exists", key);
                rs.CompletedSuccessfully = true;
                rs.RequiresConfigUpdate = false;
            }
            return rs;
        }

        private static void AddEntityFrameworkConfigSections(XmlDocument webconfig)
        {

            // get the configSections
            var configSections = webconfig.SelectSingleNode("configuration/configSections");

            // Create new section
            var newSection = webconfig.CreateNode(XmlNodeType.Element, "section", null);

            // Attributes
            var nameAttr = webconfig.CreateAttribute("name");
            nameAttr.Value = "entityFramework";
            var typeAttr = webconfig.CreateAttribute("type");
            typeAttr.Value = "System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            var requirePermissionAttr = webconfig.CreateAttribute("requirePermission");
            requirePermissionAttr.Value = "false";
            newSection.Attributes.Append(nameAttr);
            newSection.Attributes.Append(typeAttr);
            newSection.Attributes.Append(requirePermissionAttr);

            // Append it
            configSections.AppendChild(newSection);

            // get the configSections
            var mainConfig = webconfig.SelectSingleNode("configuration");

            // Create <entityFramework>
            var entityFramework = webconfig.CreateNode(XmlNodeType.Element, "entityFramework", null);

            // Create 
            var defaultConnectionFactory = webconfig.CreateNode(XmlNodeType.Element, "defaultConnectionFactory", null);
            var dcType = webconfig.CreateAttribute("type");
            dcType.Value = "System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework";
            defaultConnectionFactory.Attributes.Append(dcType);
            entityFramework.AppendChild(defaultConnectionFactory);

            // Create Providers
            var providers = webconfig.CreateNode(XmlNodeType.Element, "providers", null);

            // Create Provider element
            var provider = webconfig.CreateNode(XmlNodeType.Element, "provider", null);
            var provinvariantName = webconfig.CreateAttribute("invariantName");
            provinvariantName.Value = "System.Data.SqlClient";
            provider.Attributes.Append(provinvariantName);
            var provType = webconfig.CreateAttribute("type");
            provType.Value = "System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer";
            provider.Attributes.Append(provType);

            // Append Provide to Providers
            providers.AppendChild(provider);

            // Append Providers 
            entityFramework.AppendChild(providers);

            // Append Providers 
            mainConfig.AppendChild(entityFramework);

            //<entityFramework>
            //    <defaultConnectionFactory type="System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework" />
            //    <providers>
            //        <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
            //    </providers>
            //</entityFramework>

        }
    }
}