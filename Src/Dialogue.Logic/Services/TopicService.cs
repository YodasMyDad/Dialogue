namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Application;
    using Data.Context;
    using Mapping;
    using Models;
    using Umbraco.Core;

    public partial class TopicService : IRequestCachedService
    {



        #region Populate Methods

        public void PopulateMembers(IList<Topic> entityList)
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

        public void PopulateCategories(IList<Topic> entityList)
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

        public void PopulateLastPostMembers(IList<Topic> entityList)
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

        public void PopulateAll(IList<Topic> entityList)
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
            return ContextPerRequest.Db.Topic.AsNoTracking().ToList();
        }

        public IList<Topic> GetHighestViewedTopics(int amountToTake)
        {
            var topics = ContextPerRequest.Db.Topic.AsNoTracking()
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
            var topics = ContextPerRequest.Db.Topic.AsNoTracking()
                            .Where(x => x.Slug.Contains(slug))
                            .Include(x => x.LastPost)
                            .ToList();

            PopulateAll(topics);

            return topics;
        }

        public List<string> GetTopicBySlugUrls(string slug)
        {

            return ContextPerRequest.Db.Topic.AsNoTracking()
                            .Where(x => x.Slug.StartsWith(slug))
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
            var topics = ContextPerRequest.Db.Topic.AsNoTracking()
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
        /// <param name="postService"></param>
        /// <returns></returns>
        public Topic AddLastPost(Topic topic, string postContent, PostService postService)
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
            postService.Add(post);

            // Assign to topic
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
            var total = ContextPerRequest.Db.Topic.AsNoTracking().Count(x => x.Pending != true);
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Topic.AsNoTracking()
                                .Where(x => x.Pending != true)
                                .Include(x => x.LastPost)
                                .Include(x => x.Posts.Select(v => v.Votes))
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
            var results = ContextPerRequest.Db.Topic.AsNoTracking()
                                .Where(x => x.Pending != true)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Include(x => x.LastPost)
                                .OrderByDescending(s => s.CreateDate)
                                .Take(amountToTake)
                                .ToList();

            PopulateAll(results);

            return results;
        }

        /// <summary>
        /// Returns a specified amount of most recent topics in a list used for RSS feeds
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="catIds">onlys shows topics from these categories</param>
        /// <returns></returns>
        public IList<Topic> GetRecentRssTopics(int amountToTake, List<int> catIds)
        {
            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Topic.AsNoTracking()
                                .Where(x => x.Pending != true)
                                .Where(x => catIds.Contains(x.CategoryId))
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Include(x => x.LastPost)
                                .OrderByDescending(s => s.CreateDate)
                                .Take(amountToTake)
                                .ToList();

            PopulateAll(results);

            return results;
        }

        /// <summary>
        /// Gets ALL Topics including pending ones
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="populateAll"></param>
        /// <returns></returns>
        public IList<Topic> GetAllTopicsByUser(int memberId, bool populateAll = false)
        {
            var results = ContextPerRequest.Db.Topic
                                .Where(x => x.MemberId == memberId)
                                .ToList();

            if (populateAll)
            {
                PopulateAll(results);
            }
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
            var results = ContextPerRequest.Db.Topic.AsNoTracking()
                                .Where(x => x.CategoryId == categoryId)
                                .Where(x => x.Pending != true)
                                .Include(x => x.LastPost)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .OrderByDescending(x => x.IsSticky)
                                .ThenByDescending(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            PopulateAll(results);

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public List<Topic> GetAllPendingTopics()
        {

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Topic.AsNoTracking()
                                .Where(x => x.Pending)
                                .Include(x => x.LastPost)
                                .OrderBy(x => x.LastPost.DateCreated)
                                .ToList();

            PopulateAll(results);


            return results;
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
            var results = ContextPerRequest.Db.Topic.AsNoTracking()
                                .Where(x => x.Pending == true)
                                .Include(x => x.LastPost)
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

            var topics = ContextPerRequest.Db.Topic.AsNoTracking()
                            .Where(x => x.CategoryId == categoryId)
                            .Where(x => x.Pending != true)
                            .Include(x => x.LastPost)
                            .Include(x => x.Posts.Select(v => v.Votes))
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
            var total = ContextPerRequest.Db.Post.AsNoTracking()
                .Where(x => (x.PostContent.Contains(search) | x.Topic.Name.Contains(search)))
                .Where(x => x.Pending != true)
                .Include(x => x.Topic)
                .DistinctBy(x => x.Topic.Id)
                .Count();

            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = ContextPerRequest.Db.Post.AsNoTracking()
                            .Where(x => x.PostContent.Contains(search) | x.Topic.Name.Contains(search))
                            .Where(x => x.Pending != true)
                            .Include(x => x.Topic)
                            .Include(x => x.Votes)
                            .DistinctBy(x => x.Topic.Id)
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
            var topics = ContextPerRequest.Db.Topic.AsNoTracking()
                        .Where(x => x.MemberId == memberId)
                        .Where(x => x.Pending != true)
                        .Where(x => x.Posts.Select(p => p.IsSolution).Contains(true))
                        .Include(x => x.LastPost)
                        .Include(x => x.Posts.Select(v => v.Votes))
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
        /// <param name="memberPointsService"></param>
        /// <returns>True if topic has been marked as solved</returns>
        public bool SolveTopic(Topic topic, Post post, Member marker, Member solutionWriter, MemberPointsService memberPointsService)
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
                    memberPointsService.Add(new MemberPoints
                    {
                        Points = Dialogue.Settings().PointsAddedForASolution,
                        Member = solutionWriter,
                        MemberId = solutionWriter.Id,
                        RelatedPostId = post.Id
                    });
                }

                solved = true;
            }


            return solved;
        }
    }
}