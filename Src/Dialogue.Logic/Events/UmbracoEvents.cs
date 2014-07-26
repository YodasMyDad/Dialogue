using System.Web.Routing;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Installation;
using Dialogue.Logic.Routes;
using Dialogue.Logic.Services;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Trees;
using umbraco.cms.businesslogic.packager;
using MemberService = Umbraco.Core.Services.MemberService;
using System;

namespace Dialogue.Logic.Events
{

    public class UmbracoEvents : IApplicationEventHandler
    {

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //throw new NotImplementedException();
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {

        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Map the Custom routes
            DialogueRoutes.MapRoutes(RouteTable.Routes, UmbracoContext.Current.ContentCache);

            //Check to see if section needs to be added
            InstallHelpers.AddSection(applicationContext);

            //Check to see if language keys for section needs to be added
            InstallHelpers.AddSectionLanguageKeys();

            //Add Section Dashboard XML
            //Install.AddSectionDashboard();

            //Add OLD Style Package Event
            InstalledPackage.BeforeDelete += InstalledPackageBeforeDelete;

            //Add Tree Node Rendering Event - Used to check if user is admin to display settings node in tree
            TreeControllerBase.TreeNodesRendering += TreeControllerBaseTreeNodesRendering;

            // Fire Custom Events Here
            //ContentService.Saved += ContentServiceSaved;
            //ContentService.Published += ContentServiceOnPublished;
            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleting += MemberServiceOnDeleting;

            // Sync the badges
            // Do the badge processing
            var badgeService = new BadgeService();
            var unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
            using (var unitOfWork = unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    badgeService.SyncBadges();
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    AppHelpers.LogError(string.Format("Error processing badge classes: {0}", ex.Message));
                }
            }

        }

        private void MemberServiceOnDeleting(IMemberService sender, DeleteEventArgs<IMember> deleteEventArgs)
        {           

            var memberService = new Services.MemberService();
            var unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
            using (var unitOfWork = unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    foreach (var member in deleteEventArgs.DeletedEntities)
                    {
                        var canDelete = memberService.DeleteAllAssociatedMemberInfo(member.Id, unitOfWork);
                        if (!canDelete)
                        {
                            deleteEventArgs.Cancel = true;
                            break;
                            //TODO - Need to show a notification to the user
                        }
                    }

                }
                catch (Exception ex)
                {
                    AppHelpers.LogError("Error attempting to delete members", ex);
                }
            }
        }

        static void MemberServiceSaved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            var mService = new Services.MemberService();
            foreach (var entity in e.SavedEntities)
            {
                if (entity.HasProperty(AppConstants.PropMemberEmail))
                {
                    entity.SetValue(AppConstants.PropMemberEmail, entity.Email);

                    string previousSlug = null;
                    if (entity.Properties[AppConstants.PropMemberSlug].Value != null)
                    {
                        previousSlug = entity.Properties[AppConstants.PropMemberSlug].Value.ToString();
                    }
                    entity.SetValue(AppConstants.PropMemberSlug, AppHelpers.GenerateSlug(entity.Username,
                                                                                          mService.GetMembersWithSameSlug(AppHelpers.CreateUrl(entity.Username)),
                                                                                          previousSlug));
                    sender.Save(entity, false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void TreeControllerBaseTreeNodesRendering(TreeControllerBase sender, TreeNodesRenderingEventArgs e)
        {
            ////Get Current User
            //var currentUser = User.GetCurrent(); // Use obsolete as this method doesnt exist in new services
            ////AppHelpers.UmbServices().UserService.();

            ////This will only run on the analyticsTree & if the user is NOT admin
            //if (sender.TreeAlias == "dialogueTree" && !currentUser.IsAdmin())
            //{
            //    //setting node to remove
            //    var settingNode = e.Nodes.SingleOrDefault(x => x.Id.ToString() == "settings");

            //    //Ensure we found the node
            //    if (settingNode != null)
            //    {
            //        //Remove the settings node from the collection
            //        e.Nodes.Remove(settingNode);
            //    }
            //}
        }

        /// <summary>
        /// Uninstall Package - Before Delete (Old style events, no V6/V7 equivelant)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void InstalledPackageBeforeDelete(InstalledPackage sender, System.EventArgs e)
        {
            //Check which package is being uninstalled
            if (sender.Data.Name == "Dialogue")
            {
                //Start Uninstall - clean up process...
                UnInstallHelpers.RemoveSection();
                UnInstallHelpers.RemoveSectionLanguageKeys();
            }
        }

        private void ContentServiceOnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> publishEventArgs)
        {

        }

        private void ContentServiceSaved(IContentService sender, SaveEventArgs<IContent> saveEventArgs)
        {


        }
    }
}