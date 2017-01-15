namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System.Collections.Generic;
    using System.Linq;
    using Data.Context;
    using Models;

    public partial class CategoryNotificationService : IRequestCachedService
    {
        public IList<CategoryNotification> GetAll()
        {
            return ContextPerRequest.Db.CategoryNotification.ToList();
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        /// <param name="notification"></param>
        public void Delete(CategoryNotification notification)
        {
            ContextPerRequest.Db.CategoryNotification.Remove(notification);
        }

        /// <summary>
        /// Return all notifications by a specified category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByCategory(Category category)
        {
            return ContextPerRequest.Db.CategoryNotification.AsNoTracking().Where(x => x.CategoryId == category.Id).ToList();
        }

        /// <summary>
        /// Return notifications for a specified user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByUser(Member user)
        {
            return GetByUser(user.Id);
        }
        public IList<CategoryNotification> GetByUser(int memberId)
        {
            return ContextPerRequest.Db.CategoryNotification.Where(x => x.MemberId == memberId).ToList();
        }

        /// <summary>
        /// Return notifications for a specified user and category
        /// </summary>
        /// <param name="user"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByUserAndCategory(Member user, Category category)
        {
            return ContextPerRequest.Db.CategoryNotification.Where(x => x.CategoryId == category.Id && x.MemberId == user.Id).ToList();
        }

        /// <summary>
        /// Add a new category notification
        /// </summary>
        /// <param name="category"></param>
        public void Add(CategoryNotification category)
        {
            ContextPerRequest.Db.CategoryNotification.Add(category);

        }
    }
}