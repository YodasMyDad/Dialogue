using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class CategoryNotificationService
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