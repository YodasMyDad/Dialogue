using System.Linq;
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
using Umbraco.Core.Sync;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Routing;
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
            UrlProviderResolver.Current.AddType<VirtualNodeUrlProvider>();
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Map the Custom routes
            DialogueRoutes.MapRoutes(RouteTable.Routes, UmbracoContext.Current.ContentCache);

            //list to the init event of the application base, this allows us to bind to the actual HttpApplication events
            UmbracoApplicationBase.ApplicationInit += UmbracoApplicationBase_ApplicationInit;

            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleting += MemberServiceOnDeleting;
            ContentService.Trashing +=ContentService_Trashing;

            PageCacheRefresher.CacheUpdated += PageCacheRefresher_CacheUpdated;

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


        private static void ContentService_Trashing(IContentService sender, MoveEventArgs<IContent> e)
        {
            // TODO - Stop Categories being deleted if they have topics still using them
            // TODO - Log it, notifications don't work
            foreach (var item in e.MoveInfoCollection)
            {
                if (item.Entity.ContentType.Alias == AppConstants.DocTypeForumCategory)
                {
                    // This is a category attempting to be moved to the Recycle bin
                    // See if any topics are currently using the category
                    var topics = ServiceFactory.TopicService.GetPagedTopicsByCategory(1, Int32.MaxValue, Int32.MaxValue, item.Entity.Id);
                    if (topics.Any())
                    {
                        // We have topics using it, so stop the move
                        e.Cancel = true;

                        //TODO - Still can't create a notification so just log it
                        AppHelpers.LogError(string.Format("Unable to delete Category, it's currently being used by {0} topic(s)", topics.Count));
                    }
                }
            }
        }


        /// <summary>
        /// Bind to the PostRequestHandlerExecute event of the HttpApplication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UmbracoApplicationBase_ApplicationInit(object sender, EventArgs e)
        {
            var app = (UmbracoApplicationBase)sender;
            app.PostRequestHandlerExecute += UmbracoApplication_PostRequestHandlerExecute;
        }

        /// <summary>
        /// At the end of a request, we'll check if there is a flag in the request indicating to rebuild the routes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// In some cases many articulate roots might be published at one time but we only want to rebuild the routes once so we'll do it once
        /// at the end of the request.
        /// </remarks>
        private static void UmbracoApplication_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (ApplicationContext.Current == null) return;
            if (ApplicationContext.Current.ApplicationCache.RequestCache.GetCacheItem("dialogue-refresh-routes") == null) return;
            //the token was found so that means one or more articulate root nodes were changed in this request, rebuild the routes.
            DialogueRoutes.MapRoutes(RouteTable.Routes, UmbracoContext.Current.ContentCache);
        }

        /// <summary>
        /// When the page cache is refreshed, we'll check if any articulate root nodes were included in the refresh, if so we'll set a flag
        /// on the current request to rebuild the routes at the end of the request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This will also work for load balanced scenarios since this event executes on all servers
        /// </remarks>
        private static void PageCacheRefresher_CacheUpdated(PageCacheRefresher sender, Umbraco.Core.Cache.CacheRefresherEventArgs e)
        {
            if (UmbracoContext.Current == null) return;

            switch (e.MessageType)
            {
                case MessageType.RefreshById:
                case MessageType.RemoveById:
                    var item = UmbracoContext.Current.ContentCache.GetById((int)e.MessageObject);
                    if (item != null && item.DocumentTypeAlias.InvariantEquals("Dialogue"))
                    {
                        //add the unpublished entities to the request cache
                        ApplicationContext.Current.ApplicationCache.RequestCache.GetCacheItem("dialogue-refresh-routes", () => true);
                    }
                    break;
                case MessageType.RefreshByInstance:
                case MessageType.RemoveByInstance:
                    var content = e.MessageObject as IContent;
                    if (content == null) return;
                    if (content.ContentType.Alias.InvariantEquals("Dialogue"))
                    {
                        //add the unpublished entities to the request cache
                        UmbracoContext.Current.Application.ApplicationCache.RequestCache.GetCacheItem("dialogue-refresh-routes", () => true);
                    }
                    break;
            }
        }

        private static void MemberServiceOnDeleting(IMemberService sender, DeleteEventArgs<IMember> deleteEventArgs)
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
                            //TODO - THIS DOESN'T WORK - JUST LOG IT
                            //var clientTool = new ClientTools((Page)HttpContext.Current.CurrentHandler);
                            //clientTool.ShowSpeechBubble(SpeechBubbleIcon.Error, "Error", "Unable to delete member. Check logfile for further information");
                            AppHelpers.LogError(string.Format("There was an error attemping to delete member {0} and all of their associated data (Posts, Topics etc...)", member.Name));

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

        private static void MemberServiceSaved(IMemberService sender, SaveEventArgs<IMember> e)
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