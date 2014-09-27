using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class PrivateMessageService
    {
        public void PopulateMembers(IList<PrivateMessage> entityList)
        {
            if (entityList != null && entityList.Any())
            {
                // Map full Members
                var membersIds = entityList.Select(x => x.MemberFromId).ToList();
                membersIds.AddRange(entityList.Select(x => x.MemberToId));
                var members = MemberMapper.MapMember(membersIds.Distinct().ToList());
                foreach (var entity in entityList)
                {
                    var memberFrom = members.FirstOrDefault(x => x.Id == entity.MemberFromId);
                    var memberTo = members.FirstOrDefault(x => x.Id == entity.MemberToId);
                    entity.MemberFrom = memberFrom;
                    entity.MemberTo = memberTo;
                }
            }

        }

        public PrivateMessage SanitizeMessage(PrivateMessage privateMessage)
        {
            privateMessage.Message = AppHelpers.GetSafeHtml(privateMessage.Message);
            privateMessage.Subject = AppHelpers.SafePlainText(privateMessage.Subject);
            return privateMessage;
        }


        /// <summary>
        /// Add a private message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public PrivateMessage Add(PrivateMessage message)
        {
            // This is the message that the other user sees
            message = SanitizeMessage(message);
            message.DateSent = DateTime.UtcNow;
            message.IsSentMessage = false;
            var origMessage = ContextPerRequest.Db.PrivateMessage.Add(message);

            // We create a sent message that sits in the users sent folder, this is 
            // so that if the receiver deletes the message - The sender still has a record of it.
            var sentMessage = new PrivateMessage
            {
                IsSentMessage = true,
                DateSent = message.DateSent,
                Message = message.Message,
                Subject = message.Subject,
                MemberFromId = message.MemberFromId,
                MemberToId = message.MemberToId
            };
            ContextPerRequest.Db.PrivateMessage.Add(sentMessage);

            // Return the main message
            return origMessage;
        }

        /// <summary>
        /// Return a private message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PrivateMessage Get(Guid id)
        {
            var message = ContextPerRequest.Db.PrivateMessage.FirstOrDefault(x => x.Id == id);
            if (message != null)
            {
                PopulateMembers(new List<PrivateMessage> { message });
            }
            return message;
        }

        /// <summary>
        /// Return list of paged private messages by sent user
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public PagedList<PrivateMessage> GetPagedSentMessagesByUser(int pageIndex, int pageSize, Member user)
        {
            var totalCount = ContextPerRequest.Db.PrivateMessage.Count(x => x.MemberFromId == user.Id);

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.PrivateMessage.AsNoTracking()
                                .Where(x => x.MemberFromId == user.Id)
                                .Where(x => x.IsSentMessage == true)
                                .OrderByDescending(x => x.DateSent)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            PopulateMembers(results);

            // Return a paged list
            return new PagedList<PrivateMessage>(results, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Return list of paged private messages by received user
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public PagedList<PrivateMessage> GetPagedReceivedMessagesByUser(int pageIndex, int pageSize, Member user)
        {
            var totalCount = ContextPerRequest.Db.PrivateMessage.Count(x => x.MemberToId == user.Id);

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.PrivateMessage.AsNoTracking()
                                .Where(x => x.MemberToId == user.Id)
                                .Where(x => x.IsSentMessage != true)
                                .OrderByDescending(x => x.DateSent)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();
            PopulateMembers(results);

            // Return a paged list
            return new PagedList<PrivateMessage>(results, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Gets the last sent private message from a specific user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PrivateMessage GetLastSentPrivateMessage(int id)
        {
            var message = ContextPerRequest.Db.PrivateMessage.AsNoTracking().FirstOrDefault(x => x.MemberFromId == id);
            if (message != null)
            {
                PopulateMembers(new List<PrivateMessage> { message });
            }
            return message;
        }

        public PrivateMessage GetMatchingSentPrivateMessage(string title, DateTime date, int senderId, int receiverId)
        {
            var message = ContextPerRequest.Db.PrivateMessage.AsNoTracking()
                .FirstOrDefault(x => x.Subject == title && x.DateSent == date && x.MemberFromId == senderId && x.MemberToId == receiverId && x.IsSentMessage == true);
            if (message != null)
            {
                PopulateMembers(new List<PrivateMessage> { message });
            }
            return message;
        }

        /// <summary>
        /// Gets all private messages sent by a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllSentByUser(int id)
        {
            var results = ContextPerRequest.Db.PrivateMessage.AsNoTracking()
                                .Where(x => x.MemberFromId == id)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
            PopulateMembers(results);
            return results;
        }

        public IList<PrivateMessage> GetAllByUserSentOrReceived(int id)
        {
            var results = ContextPerRequest.Db.PrivateMessage
                                .Where(x => x.MemberFromId == id || x.MemberToId == id)
                                .ToList();
            PopulateMembers(results);
            return results;
        }

        /// <summary>
        /// Returns a count of any new messages the user has
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int NewPrivateMessageCount(int userId)
        {
            return ContextPerRequest.Db.PrivateMessage.AsNoTracking().Count(x => x.MemberToId == userId && !x.IsRead && x.IsSentMessage != true);
        }

        /// <summary>
        /// Gets all private messages received by a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllReceivedByUser(int id)
        {
            var results = ContextPerRequest.Db.PrivateMessage.AsNoTracking()
                                .Where(x => x.MemberToId == id)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
            PopulateMembers(results);
            return results;
        }


        /// <summary>
        /// get all private messages sent from one user to another
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllByUserToAnotherUser(int senderId, int receiverId)
        {
            var results = ContextPerRequest.Db.PrivateMessage
                                .Where(x => x.MemberFromId == senderId && x.MemberToId == receiverId)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
            PopulateMembers(results);
            return results;
        }

        /// <summary>
        /// Delete a private message
        /// </summary>
        /// <param name="message"></param>
        public void DeleteMessage(PrivateMessage message)
        {
            ContextPerRequest.Db.PrivateMessage.Remove(message);
        }
    }
}