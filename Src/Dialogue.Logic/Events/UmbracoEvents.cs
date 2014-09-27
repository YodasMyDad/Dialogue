using System.Web;
using System.Web.Routing;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Routes;
using Dialogue.Logic.Services;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.UI;
using Umbraco.Web.UI.Pages;
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
            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleting += MemberServiceOnDeleting;

            // Sync the badges
            // Do the badge processing
            var unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
            using (var unitOfWork = unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    ServiceFactory.BadgeService.SyncBadges();
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
                            //TODO - Check this notification works - It doesn't!! Need to sort
                            //DialogueService??
                            var basePage = ((BasePage)HttpContext.Current.Handler);
                            basePage.ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Error, "Error", "Unable to delete member. Check logfile for further information");
                            break;

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

    }
}