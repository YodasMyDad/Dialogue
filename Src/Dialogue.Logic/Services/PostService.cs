using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public class PostService
    {
        private readonly PermissionService _permissionService;
        private readonly MemberPointsService _memberPointsService;
        public PostService()
        {
            _permissionService = new PermissionService();
            _memberPointsService = new MemberPointsService();
        }

        #region Populate Methods

        private static void PopulateMembers(IList<Post> entityList)
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

        #endregion

        public Post Add(Post post)
        {
            post = SanitizePost(post);
            return ContextPerRequest.Db.Post.Add(post);
        }

        public Post SanitizePost(Post post)
        {
            post.PostContent = AppHelpers.GetSafeHtml(post.PostContent);
            return post;
        }

        public Post GetTopicStarterPost(Guid topicId)
        {
            return ContextPerRequest.Db.Post
                .FirstOrDefault(x => x.Topic.Id == topicId && x.IsTopicStarter);
        }

        /// <summary>
        /// Return all posts
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Post> GetAll()
        {
            return ContextPerRequest.Db.Post;
        }

        /// <summary>
        /// Returns a list of posts ordered by the lowest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetLowestVotedPost(int amountToTake)
        {
            return ContextPerRequest.Db.Post
                .Include(x => x.Votes)
                .Where(x => x.VoteCount < 0 && x.Pending != true)
                .OrderBy(x => x.VoteCount)
                .Take(amountToTake)
                .ToList();
        }

        /// <summary>
        /// Returns a list of posts ordered by the highest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetHighestVotedPost(int amountToTake)
        {
            return ContextPerRequest.Db.Post
                .Include(x => x.Votes)
                .Where(x => x.VoteCount > 0 && x.Pending != true)
                .OrderByDescending(x => x.VoteCount)
                .Take(amountToTake)
                .ToList();
        }

        /// <summary>
        /// Return all posts by a specified member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetByMember(int memberId, int amountToTake)
        {
            return ContextPerRequest.Db.Post
                .Include(x => x.Votes)
                .Where(x => x.MemberId == memberId && x.Pending != true)
                .OrderByDescending(x => x.DateCreated)
                .Take(amountToTake)
                .ToList();
        }

        /// <summary>
        /// Returns a paged list of posts by a search term
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm)
        {
            var search = AppHelpers.SafePlainText(searchTerm);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = ContextPerRequest.Db.Post.Count(x => x.PostContent.Contains(search) | x.Topic.Name.Contains(search));
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts
            var results = ContextPerRequest.Db.Post
                            .Include(x => x.Votes)
                            .Where(x => x.PostContent.Contains(search) | x.Topic.Name.Contains(search))
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            
            PopulateMembers(results);

            // Return a paged list
            return new PagedList<Post>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Returns a paged list of posts by a topic id
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="topicId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = ContextPerRequest.Db.Post
                                    .Include(x => x.Topic)
                                    .Count(x => x.Topic.Id == topicId && x.Pending != true);
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Post
                                    .Include(x => x.Topic)
                                    .Include(x => x.Votes)
                                    .Include(x => x.Files)
                                  .Where(x => x.Topic.Id == topicId && !x.IsTopicStarter && x.Pending != true);

            // Sort what order the posts are sorted in
            switch (order)
            {
                case PostOrderBy.Newest:
                    results = results.OrderByDescending(x => x.DateCreated);
                    break;

                case PostOrderBy.Votes:
                    results = results.OrderByDescending(x => x.VoteCount).ThenBy(x => x.DateCreated);
                    break;

                default:
                    results = results.OrderBy(x => x.DateCreated);
                    break;
            }

            // sort the paging out
            var posts = results.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            PopulateMembers(posts);

            // Return a paged list
            return new PagedList<Post>(posts, pageIndex, pageSize, total);
        }

        public PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize)
        {

            var total = ContextPerRequest.Db.Post.Count(x => x.Pending == true);
            var results = ContextPerRequest.Db.Post.Include(x => x.Topic)
                    .Include(x => x.Files)
                .Where(x => x.Pending == true)
                .OrderBy(x => x.DateCreated)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            PopulateMembers(results);

            return new PagedList<Post>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Return all posts by a specified member that are marked as solution
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IList<Post> GetSolutionsByMember(int memberId)
        {
            return ContextPerRequest.Db.Post
                .Include(x => x.Votes)
                .Where(x => x.MemberId == memberId)
                .Where(x => x.IsSolution && x.Pending != true)
                .OrderByDescending(x => x.DateCreated)
                .ToList();
        }

        /// <summary>
        /// Returns a count of all posts
        /// </summary>
        /// <returns></returns>
        public int PostCount()
        {
            return GetAll().Count();
        }

        /// <summary>
        /// Return a post by Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public Post Get(Guid postId)
        {
            return ContextPerRequest.Db.Post.FirstOrDefault(x => x.Id == postId);
        }


        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="post"></param>
        /// <returns> True if parent topic should now be deleted (caller's responsibility)</returns>
        public bool Delete(Post post)
        {
            // Here is where we can check for reasons not to delete the post
            // And change the value below if not

            var deleteTopic = false;

            // Before we delete the post, we need to check if this is the last post in the topic
            // and if so update the topic
            var topic = post.Topic;
            var lastPost = topic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();

            if (lastPost != null && lastPost.Id == post.Id)
            {
                // Get the new last post and update the topic
                topic.LastPost = topic.Posts.Where(x => x.Id != post.Id).OrderByDescending(x => x.DateCreated).FirstOrDefault();
            }

            if (topic.Solved && post.IsSolution)
            {
                topic.Solved = false;
            }

            topic.Posts.Remove(post);

            deleteTopic = post.IsTopicStarter;

            // now delete the post
            ContextPerRequest.Db.Post.Remove(post);

            // Topic should be deleted, so make sure it has no last post to avoid circular dependency
            if (deleteTopic)
            {
                topic.LastPost = null;
            }

            return deleteTopic;
        }


        /// <summary>
        /// Add a new post
        /// </summary>
        /// <param name="postContent"> </param>
        /// <param name="topic"> </param>
        /// <param name="user"></param>
        /// <param name="permissions"> </param>
        /// <returns>True if post added</returns>
        public Post AddNewPost(string postContent, Topic topic, Member user, out PermissionSet permissions)
        {
            // Get the permissions for the category that this topic is in
            permissions = _permissionService.GetPermissions(topic.Category, user.Groups.FirstOrDefault());

            // Check this users role has permission to create a post
            if (permissions[AppConstants.PermissionDenyAccess].IsTicked || permissions[AppConstants.PermissionReadOnly].IsTicked)
            {
                // Throw exception so Ajax caller picks it up
                throw new ApplicationException(AppHelpers.Lang("Errors.NoPermission"));
            }

            // Has permission so create the post
            var newPost = new Post
            {
                PostContent = postContent,
                Member = user,
                MemberId = user.Id,
                Topic = topic,
                IpAddress = AppHelpers.GetUsersIpAddress()
            };

            newPost = SanitizePost(newPost);

            var category = topic.Category;
            if (category.ModerateAllPostsInThisCategory == true)
            {
                newPost.Pending = true;
            }


            // create the post
            Add(newPost);

            // Update the users points score and post count for posting
            _memberPointsService.Add(new MemberPoints
            {
                Points = Dialogue.Settings().PointsAddedPerNewPost,
                Member = user
            });

            // add the last post to the topic
            topic.LastPost = newPost;

            return newPost;
        }

    }
}