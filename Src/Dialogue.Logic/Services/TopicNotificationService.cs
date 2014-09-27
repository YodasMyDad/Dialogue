using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class TopicNotificationService
    {

        #region Populate Methods

        private List<TopicNotification> PopulateMembers(List<TopicNotification> entityList)
        {
            // Map full Members
            var membersIds = entityList.Select(x => x.MemberId).ToList();
            var members = MemberMapper.MapMember(membersIds);
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberId);
                entity.Member = member;
            }

            return entityList;
        } 

        #endregion


        /// <summary>
        /// Return all topic notifications
        /// </summary>
        /// <returns></returns>
        public IList<TopicNotification> GetAll()
        {
            return ContextPerRequest.Db.TopicNotification.ToList();
        }

        /// <summary>
        /// Delete topic notification
        /// </summary>
        /// <param name="notification"></param>
        public void Delete(TopicNotification notification)
        {
            ContextPerRequest.Db.TopicNotification.Remove(notification);
        }

        /// <summary>
        /// Return all notifications for a specified topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IList<TopicNotification> GetByTopic(Topic topic)
        {
            return ContextPerRequest.Db.TopicNotification.AsNoTracking().Where(x => x.Topic.Id == topic.Id).Include(x => x.Topic).ToList();
        }

        /// <summary>
        /// Return notifications for a specified user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IList<TopicNotification> GetByUser(Member user)
        {
            return GetByUser(user.Id);
        }
        public IList<TopicNotification> GetByUser(int userId)
        {
            return ContextPerRequest.Db.TopicNotification.Where(x => x.MemberId == userId).ToList();
        }

        /// <summary>
        /// return notifications for a specified user on a specified topic
        /// </summary>
        /// <param name="user"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IList<TopicNotification> GetByUserAndTopic(Member user, Topic topic)
        {
            return ContextPerRequest.Db.TopicNotification.Where(x => x.Topic.Id == topic.Id && x.MemberId == user.Id).Include(x => x.Topic).ToList();
        }

        /// <summary>
        /// Add a new topic notification
        /// </summary>
        /// <param name="topicNotification"></param>
        public void Add(TopicNotification topicNotification)
        {
            ContextPerRequest.Db.TopicNotification.Add(topicNotification);
        }
    }
}