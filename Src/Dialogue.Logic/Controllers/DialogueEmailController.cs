using System;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.Controllers
{
    #region Surface Controllers
    public partial class DialogueEmailSurfaceController : BaseSurfaceController
    {
        [HttpPost]
        [Authorize]
        public void Subscribe(SubscribeEmailViewModel subscription)
        {
            if (Request.IsAjaxRequest())
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        // Add logic to add subscr
                        var isCategory = subscription.SubscriptionType.Contains("category");
                        var id = subscription.Id;

                        if (isCategory)
                        {
                            // get the category
                            var cat = ServiceFactory.CategoryService.Get(Convert.ToInt32(id));

                            if (cat != null)
                            {

                                // Create the notification
                                var categoryNotification = new CategoryNotification
                                {
                                    Category = cat,
                                    CategoryId = cat.Id,
                                    Member = CurrentMember,
                                    MemberId = CurrentMember.Id
                                };
                                //save

                                ServiceFactory.CategoryNotificationService.Add(categoryNotification);
                            }
                        }
                        else
                        {
                            // get the category
                            var topic = ServiceFactory.TopicService.Get(new Guid(id));

                            // check its not null
                            if (topic != null)
                            {

                                // Create the notification
                                var topicNotification = new TopicNotification
                                {
                                    Topic = topic,
                                    Member = CurrentMember,
                                    MemberId = CurrentMember.Id
                                };
                                ServiceFactory.TopicNotificationService.Add(topicNotification);
                            }
                        }

                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LogError(ex);
                        throw new Exception(Lang("Errors.GenericMessage"));
                    }
                }
            }
            else
            {
                throw new Exception(Lang("Errors.GenericMessage"));
            }
        }

        [HttpPost]
        [Authorize]
        public void UnSubscribe(UnSubscribeEmailViewModel subscription)
        {
            if (Request.IsAjaxRequest())
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        // Add logic to add subscr
                        var isCategory = subscription.SubscriptionType.Contains("category");
                        var id = subscription.Id;

                        if (isCategory)
                        {
                            // get the category
                            var cat = ServiceFactory.CategoryService.Get(Convert.ToInt32(id));

                            if (cat != null)
                            {
                                // get the notifications by user
                                var notifications = ServiceFactory.CategoryNotificationService.GetByUserAndCategory(CurrentMember, cat);

                                if (notifications.Any())
                                {
                                    foreach (var categoryNotification in notifications)
                                    {
                                        // Delete
                                        ServiceFactory.CategoryNotificationService.Delete(categoryNotification);
                                    }
                                }

                            }
                        }
                        else
                        {
                            // get the topic
                            var topic = ServiceFactory.TopicService.Get(new Guid(id));

                            if (topic != null)
                            {
                                // get the notifications by user
                                var notifications = ServiceFactory.TopicNotificationService.GetByUserAndTopic(CurrentMember, topic);

                                if (notifications.Any())
                                {
                                    foreach (var topicNotification in notifications)
                                    {
                                        // Delete
                                        ServiceFactory.TopicNotificationService.Delete(topicNotification);
                                    }
                                }

                            }
                        }

                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LogError(ex);
                        throw new Exception(Lang("Errors.GenericMessage"));
                    }
                }
            }
            else
            {
                throw new Exception(Lang("Errors.GenericMessage"));
            }
        }
    } 
    #endregion
}