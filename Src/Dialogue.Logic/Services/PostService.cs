namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Entity;
    using Application;
    using Constants;
    using Data.Context;
    using Data.UnitOfWork;
    using Mapping;
    using Models;

    public partial class PostService : IRequestCachedService
    {

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
            var topicStarter = ContextPerRequest.Db.Post.AsNoTracking()
                .Where(x => x.Topic.Id == topicId && x.IsTopicStarter).Include(x => x.Topic).FirstOrDefault();
            PopulateMembers(new List<Post> { topicStarter });
            return topicStarter;
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
            var posts = ContextPerRequest.Db.Post.AsNoTracking()
                .Where(x => x.VoteCount < 0 && x.Pending != true)
                .OrderBy(x => x.VoteCount)
                .Take(amountToTake)
                .Include(x => x.Votes)
                .Include(x => x.Topic)
                .ToList();

            PopulateMembers(posts);

            return posts;
        }

        /// <summary>
        /// Returns a list of posts ordered by the highest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetHighestVotedPost(int amountToTake)
        {
            return ContextPerRequest.Db.Post.AsNoTracking()
                .Where(x => x.VoteCount > 0 && x.Pending != true)
                .OrderByDescending(x => x.VoteCount)
                .Take(amountToTake)
                .Include(x => x.Votes)
                .ToList();
        }

        /// <summary>
        /// Gets ALL posts including pending
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IEnumerable<Post> GetAllByMember(int memberId)
        {
            return ContextPerRequest.Db.Post
                .Where(x => x.MemberId == memberId)
                .Include(x => x.Votes)
                .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Return all posts by a specified member ignoring pending posts
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IEnumerable<Post> GetByMember(int memberId)
        {
            return ContextPerRequest.Db.Post
                .Where(x => x.MemberId == memberId && x.Pending != true)
                .Include(x => x.Votes)
                .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Return all posts by a specified member ignoring pending posts
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public List<Post> GetByMember(int memberId, int amountToTake)
        {
            return GetByMember(memberId)
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
            var search = AppHelpers.ReturnSearchString(searchTerm);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = ContextPerRequest.Db.Post.AsNoTracking()
                        .Include(x => x.Topic)
                        .Count(x => (x.PostContent.ToUpper().Contains(search.ToUpper()) || x.Topic.Name.ToUpper().Contains(search.ToUpper()))
                            && x.Pending != true);
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts
            var results = ContextPerRequest.Db.Post.AsNoTracking()
                            .Where(x => (x.PostContent.ToUpper().Contains(search.ToUpper()) || x.Topic.Name.ToUpper().Contains(search.ToUpper())))
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Include(x => x.Topic)
                            .Include(x => x.Votes)
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
            var total = ContextPerRequest.Db.Post.AsNoTracking()
                                    .Include(x => x.Topic)
                                    .Count(x => x.Topic.Id == topicId && !x.IsTopicStarter && x.Pending != true);
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Post.AsNoTracking()
                                  .Where(x => x.Topic.Id == topicId && !x.IsTopicStarter && x.Pending != true)
                                    .Include(x => x.Topic)
                                    .Include(x => x.Votes)
                                    .Include(x => x.Files);

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

        public List<Post> GetAllPendingPosts()
        {
            var results = ContextPerRequest.Db.Post.AsNoTracking()
                .Where(x => x.Pending)
                .Include(x => x.Topic)
                .Include(x => x.Files)
                .OrderBy(x => x.DateCreated)
                .ToList();

            PopulateMembers(results);

            return results;
        }

        public PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize)
        {

            var total = ContextPerRequest.Db.Post.Count(x => x.Pending == true);
            var results = ContextPerRequest.Db.Post.AsNoTracking().Include(x => x.Topic)
                .Where(x => x.Pending)
                .OrderBy(x => x.DateCreated)
                .Include(x => x.Files)
                .Include(x => x.Topic)
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
            return ContextPerRequest.Db.Post.AsNoTracking()
                .Where(x => x.MemberId == memberId)
                .Where(x => x.IsSolution && x.Pending != true)
                .Include(x => x.Votes)
                .Include(x => x.Topic)
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
            var post = ContextPerRequest.Db.Post.Include(x => x.Topic).FirstOrDefault(x => x.Id == postId);
            PopulateMembers(new List<Post> { post });
            return post;
        }

        public List<Post> Get(List<Guid> posts)
        {
            var allPosts = ContextPerRequest.Db.Post.AsNoTracking().Include(x => x.Topic).Where(x => posts.Contains(x.Id)).ToList();
            PopulateMembers(allPosts);
            return allPosts;
        }


        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="post"></param>
        /// <param name="memberService"></param>
        /// <param name="memberPointsService"></param>
        /// <param name="topicNotificationService"></param>
        /// <returns> True if parent was deleted too</returns>
        public bool Delete(UnitOfWork unitOfWork, Post post, MemberService memberService, MemberPointsService memberPointsService, TopicNotificationService topicNotificationService)
        {
            // Get the topic
            var topic = post.Topic;

            // The member who created this post
            var postMember = memberService.Get(post.MemberId);

            var topicDeleted = false;

            // See if we need to delete the topic or not
            if (post.IsTopicStarter)
            {
                topic.LastPost = null;

                // Delete all posts
                if (topic.Posts != null)
                {
                    var postsToDelete = new List<Post>();
                    postsToDelete.AddRange(topic.Posts);
                    var memberIds = postsToDelete.Select(x => x.MemberId).Distinct().ToList();
                    foreach (var postFromTopic in postsToDelete)
                    {
                        post.Files.Clear();
                        DeleteIndividualPost(topic, postFromTopic, memberPointsService, false);
                        unitOfWork.SaveChanges();
                    }

                    // Sync the members post count. For all members who had a post deleted.
                    var members = memberService.GetAllById(memberIds);
                    memberService.SyncMembersPostCount(members);
                }

                if (topic.TopicNotifications != null)
                {
                    var notificationsToDelete = new List<TopicNotification>();
                    notificationsToDelete.AddRange(topic.TopicNotifications);
                    foreach (var topicNotification in notificationsToDelete)
                    {
                        topicNotificationService.Delete(topicNotification);
                    }
                }
                topic.Posts?.Clear();
                topic.TopicNotifications?.Clear();
                topic.Category = null;
                topic.LastPost = null;
                ContextPerRequest.Db.Topic.Remove(topic);

                // Set to true
                topicDeleted = true;
            }
            else
            {
                DeleteIndividualPost(topic, post, memberPointsService);
                memberService.SyncMembersPostCount(new List<Member> { postMember });
            }

            return topicDeleted;
        }

        /// <summary>
        /// Deletes an individual post
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="post"></param>
        /// <param name="memberPointsService"></param>
        /// <param name="resetLastPost"></param>
        private static void DeleteIndividualPost(Topic topic, Post post, MemberPointsService memberPointsService, bool resetLastPost = true)
        {
            if (resetLastPost)
            {
                var lastPost = topic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
                if (lastPost != null && lastPost.Id == post.Id && topic.Posts.Count > 1)
                {
                    // Get the new last post and update the topic
                    topic.LastPost = topic.Posts.Where(x => x.Id != post.Id).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                }
                else if(lastPost != null && lastPost.Id == post.Id && topic.Posts.Count <= 1)
                {
                    topic.LastPost = null;
                }

                // Mark topic as not solved if the post we are deleting was the solution
                if (topic.Solved && post.IsSolution)
                {
                    topic.Solved = false;
                }
            }

            // Remove this post from the topic so we can delete it without any errors
            topic.Posts.Remove(post);

            // Delete all the points the memeber who made this post has gained
            memberPointsService.DeletePostPoints(post);

            // now delete the post
            ContextPerRequest.Db.Post.Remove(post);
        }


        /// <summary>
        /// Add a new post
        /// </summary>
        /// <param name="postContent"> </param>
        /// <param name="topic"> </param>
        /// <param name="user"></param>
        /// <param name="permissionService"></param>
        /// <param name="categoryPermissionService"></param>
        /// <param name="memberPointsService"></param>
        /// <param name="permissions"> </param>
        /// <param name="memberService"></param>
        /// <returns>True if post added</returns>
        public Post AddNewPost(string postContent, Topic topic, Member user, PermissionService permissionService, MemberService memberService, 
                                    CategoryPermissionService categoryPermissionService, MemberPointsService memberPointsService, out PermissionSet permissions)
        {
            // Get the permissions for the category that this topic is in
            permissions = permissionService.GetPermissions(topic.Category, user.Groups.FirstOrDefault(), memberService, categoryPermissionService);

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
                IpAddress = AppHelpers.GetUsersIpAddress(),
                DateCreated = DateTime.UtcNow,
                DateEdited = DateTime.UtcNow
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
            memberPointsService.Add(new MemberPoints
            {
                Points = Dialogue.Settings().PointsAddedPerNewPost,
                MemberId = user.Id,
                RelatedPostId = newPost.Id
            });

            // add the last post to the topic
            topic.LastPost = newPost;

            // Add post to members count
            memberService.AddPostCount(user);

            return newPost;
        }

    }
}