namespace Dialogue.Logic.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Application;
    using Models;
    using Models.ViewModels;

    public partial class DialogueEmailController : DialogueBaseController
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
                            var cat = CategoryService.Get(Convert.ToInt32(id));

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

                                CategoryNotificationService.Add(categoryNotification);
                            }
                        }
                        else
                        {
                            // get the category
                            var topic = TopicService.Get(new Guid(id));

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
                                TopicNotificationService.Add(topicNotification);
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
                            var cat = CategoryService.Get(Convert.ToInt32(id));

                            if (cat != null)
                            {
                                // get the notifications by user
                                var notifications = CategoryNotificationService.GetByUserAndCategory(CurrentMember, cat);

                                if (notifications.Any())
                                {
                                    foreach (var categoryNotification in notifications)
                                    {
                                        // Delete
                                        CategoryNotificationService.Delete(categoryNotification);
                                    }
                                }

                            }
                        }
                        else
                        {
                            // get the topic
                            var topic = TopicService.Get(new Guid(id));

                            if (topic != null)
                            {
                                // get the notifications by user
                                var notifications = TopicNotificationService.GetByUserAndTopic(CurrentMember, topic);

                                if (notifications.Any())
                                {
                                    foreach (var topicNotification in notifications)
                                    {
                                        // Delete
                                        TopicNotificationService.Delete(topicNotification);
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

}