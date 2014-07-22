using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public class TopicService
    {
        private readonly PostService _postService;
        private readonly TopicNotificationService _topicNotificationService;
        private readonly MemberPointsService _memberPointsService;
        public TopicService()
        {
            _postService = new PostService();
            _topicNotificationService = new TopicNotificationService();
            _memberPointsService = new MemberPointsService();
        }

        #region Populate Methods

        private static void PopulateMembers(IList<Topic> entityList)
        {
            // Map full Members
            var membersIds = entityList.Select(x => x.MemberId).ToList();
            var members = MemberMapper.MapMember(membersIds);
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberId);
                entity.Member = member;
            }
        }

        private static void PopulateCategories(IList<Topic> entityList)
        {
            // Map full categories
            var catIds = entityList.Select(x => x.CategoryId).ToList();
            var cats = CategoryMapper.MapCategory(catIds);
            foreach (var entity in entityList)
            {
                var cat = cats.FirstOrDefault(x => x.Id == entity.CategoryId);
                entity.Category = cat;
            }
        }

        private static void PopulateLastPostMembers(IList<Topic> entityList)
        {
            // Map full categories
            var membersIds = entityList.Select(x => x.LastPost.MemberId).ToList();
            var members = MemberMapper.MapMember(membersIds);
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.LastPost.MemberId);
                entity.LastPost.Member = member;
            }
        }

        private static void PopulateAll(IList<Topic> entityList)
        {
            PopulateLastPostMembers(entityList);
            PopulateCategories(entityList);
            PopulateMembers(entityList);
        }

        #endregion

        public Topic SanitizeTopic(Topic topic)
        {
            topic.Name = AppHelpers.SafePlainText(topic.Name);
            return topic;
        }

        /// <summary>
        /// Get all topics
        /// </summary>
        /// <returns></returns>
        public IList<Topic> GetAll()
        {            
            return ContextPerRequest.Db.Topic.ToList();
        }

        public IList<Topic> GetHighestViewedTopics(int amountToTake)
        {
            var topics = ContextPerRequest.Db.Topic
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.Views)
                            .Take(amountToTake)
                            .ToList();

            PopulateAll(topics);

            return topics;
        }

        /// <summary>
        /// Create a new topic and also the topic starter post
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Topic Add(Topic topic)
        {
            topic = SanitizeTopic(topic);
            topic.CreateDate = DateTime.UtcNow;

            // url slug generator
            topic.Slug = AppHelpers.GenerateSlug(topic.Name, GetTopicBySlugUrls(AppHelpers.CreateUrl(topic.Name)), null);

            return ContextPerRequest.Db.Topic.Add(topic);
        }

        public IList<Topic> GetTopicBySlugLike(string slug)
        {
            var topics = ContextPerRequest.Db.Topic
                            .Include(x => x.LastPost)
                            .Where(x => x.Slug.Contains(slug))
                            .ToList();

            PopulateAll(topics);

            return topics;
        }

        public List<string> GetTopicBySlugUrls(string slug)
        {

            return ContextPerRequest.Db.Topic
                            .Where(x => x.Slug.Contains(slug))
                            .Select(x => x.Slug)
                            .ToList();
        }

        /// <summary>
        /// Get todays topics
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Topic> GetTodaysTopics(int amountToTake)
        {
            var topics = ContextPerRequest.Db.Topic
                        .Where(c => c.CreateDate >= DateTime.Today && c.Pending != true)
                        .OrderByDescending(x => x.CreateDate)
                        .Take(amountToTake)
                        .ToList();

            PopulateAll(topics);
            return topics;
        }

        /// <summary>
        /// Add a last post to a topic. Must be part of a separate database update
        /// in EF because of circular dependencies. So save the topic before calling this.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="postContent"></param>
        /// <returns></returns>
        public Topic AddLastPost(Topic topic, string postContent)
        {
            topic = SanitizeTopic(topic);

            // Create the post
            var post = new Post
            {
                DateCreated = DateTime.UtcNow,
                IsTopicStarter = true,
                DateEdited = DateTime.UtcNow,
                PostContent = AppHelpers.GetSafeHtml(postContent),
                MemberId = topic.MemberId,
                Topic = topic
            };

            // Add the post
            _postService.Add(post);

            topic.LastPost = post;

            return topic;
        }

        /// <summary>
        /// Returns a paged list of topics, ordered by most recent
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public PagedList<Topic> GetRecentTopics(int pageIndex, int pageSize, int amountToTake)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = ContextPerRequest.Db.Topic.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Topic
                                .Include(x => x.LastPost)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Where(x => x.Pending != true)
                                .OrderByDescending(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            PopulateAll(results);

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Returns a specified amount of most recent topics in a list used for RSS feeds
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Topic> GetRecentRssTopics(int amountToTake)
        {
            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Topic
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Include(x => x.LastPost)
                                .Where(x => x.Pending != true)
                                .OrderByDescending(s => s.CreateDate)
                                .Take(amountToTake)
                                .ToList();

            PopulateAll(results);

            return results;
        }

        /// <summary>
        /// Returns all topics by a specified user
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IList<Topic> GetTopicsByUser(int memberId)
        {
            var results = ContextPerRequest.Db.Topic
                                .Where(x => x.MemberId == memberId)
                                .Where(x => x.Pending != true)
                                .ToList();

            PopulateAll(results);
            return results;
        }

        /// <summary>
        /// Returns a paged list of topics from a specified category
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public PagedList<Topic> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, int categoryId)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = ContextPerRequest.Db.Topic.Count(x => x.CategoryId == categoryId);
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Topic
                                .Include(x => x.LastPost)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Where(x => x.CategoryId == categoryId)
                                .Where(x => x.Pending != true)
                                .OrderByDescending(x => x.IsSticky)
                                .ThenByDescending(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            PopulateAll(results);

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Gets all the pending topics in a paged list
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<Topic> GetPagedPendingTopics(int pageIndex, int pageSize)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = ContextPerRequest.Db.Topic.Count(x => x.Pending == true);

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Topic
                                .Include(x => x.LastPost)
                                .Where(x => x.Pending == true)
                                .OrderBy(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            PopulateAll(results);

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Returns a specified amount of most recent topics in a category used for RSS feeds
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<Topic> GetRssTopicsByCategory(int amountToTake, int categoryId)
        {

            var topics = ContextPerRequest.Db.Topic
                            .Include(x => x.LastPost)
                            .Include(x => x.Posts.Select(v => v.Votes))
                            .Where(x => x.CategoryId == categoryId)
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.LastPost.DateCreated)
                            .Take(amountToTake)
                            .ToList();

            PopulateAll(topics);

            return topics;
        }


        /// <summary>
        /// Returns a paged amount of searched topics by a string search value
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public PagedList<Topic> SearchTopics(int pageIndex, int pageSize, int amountToTake, string searchTerm)
        {
            var search = AppHelpers.ReturnSearchString(searchTerm);
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = ContextPerRequest.Db.Post.Count(x => x.PostContent.Contains(search) | x.Topic.Name.Contains(search));
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = ContextPerRequest.Db.Post
                            .Include(x => x.Topic)
                            .Include(x => x.Votes)
                            .Where(x => x.PostContent.Contains(search) | x.Topic.Name.Contains(search))
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(x => x.Topic)
                            .ToList();

            PopulateAll(results);

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Return a topic by url slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public Topic GetTopicBySlug(string slug)
        {
            var safeSlug = AppHelpers.GetSafeHtml(slug);
            var topics = ContextPerRequest.Db.Topic
                    .Include(x => x.Poll)
                    .Include(x => x.Poll.PollAnswers)
                    .FirstOrDefault(x => x.Slug == safeSlug);

            //TODO - Check this works as we are adding it to a new list
            PopulateAll(new List<Topic> { topics });

            return topics;
        }

        /// <summary>
        /// Return a topic by Id
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public Topic Get(Guid topicId)
        {
            var topic = ContextPerRequest.Db.Topic.FirstOrDefault(x => x.Id == topicId);
            PopulateAll(new List<Topic> { topic });
            return topic;
        }

        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public void Delete(Topic topic)
        {
            topic.LastPost = null;

            // Delete all posts
            if (topic.Posts != null)
            {
                var postsToDelete = new List<Post>();
                postsToDelete.AddRange(topic.Posts);
                foreach (var post in postsToDelete)
                {
                    _postService.Delete(post);
                }
            }

            if (topic.TopicNotifications != null)
            {
                var notificationsToDelete = new List<TopicNotification>();
                notificationsToDelete.AddRange(topic.TopicNotifications);
                foreach (var topicNotification in notificationsToDelete)
                {
                    _topicNotificationService.Delete(topicNotification);
                }
            }

            ContextPerRequest.Db.Topic.Remove(topic);
        }

        public int TopicCount()
        {
            return ContextPerRequest.Db.Topic.Count();
        }

        /// <summary>
        /// Return topics by a specified user that are marked as solved
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IList<Topic> GetSolvedTopicsByMember(int memberId)
        {
            var topics = ContextPerRequest.Db.Topic
                        .Include(x => x.LastPost)
                        .Include(x => x.Posts.Select(v => v.Votes))
                        .Where(x => x.MemberId == memberId)
                        .Where(x => x.Pending != true)
                        .Where(x => x.Posts.Select(p => p.IsSolution).Contains(true))
                        .ToList();

            PopulateAll(topics);

            return topics;
        }

        /// <summary>
        /// Mark a topic as solved
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="post"></param>
        /// <param name="marker"></param>
        /// <param name="solutionWriter"></param>
        /// <returns>True if topic has been marked as solved</returns>
        public bool SolveTopic(Topic topic, Post post, Member marker, Member solutionWriter)
        {
            var solved = false;

  
                // Make sure this user owns the topic, if not do nothing
                if (topic.MemberId == marker.Id)
                {
                    // Update the post
                    post.IsSolution = true;

                    // Update the topic
                    topic.Solved = true;

                    // Assign points
                    // Do not give points to the user if they are marking their own post as the solution
                    if (marker.Id != solutionWriter.Id)
                    {
                        _memberPointsService.Add(new MemberPoints
                        {
                            Points = Dialogue.Settings().PointsAddedForASolution,
                            Member = solutionWriter
                        });
                    }

                    solved = true;
                }
      

            return solved;
        }
    }
}