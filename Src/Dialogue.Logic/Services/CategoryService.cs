namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Constants;
    using Mapping;
    using Models;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Web;

    public partial class CategoryService : IRequestCachedService
    {
        private readonly IPublishedContent _forumRootNode;

        public CategoryService()
        {
            _forumRootNode = AppHelpers.GetNode(Dialogue.Settings().ForumId);
        }
        public Category Get(int id, bool getSubAndParentCats = false)
        {
            return CategoryMapper.MapCategory(AppHelpers.GetNode(id), getSubAndParentCats);
        }

        public List<Category> Get(List<int> ids)
        {
            var cats = new List<Category>();
            if (ids.Any())
            {
                var allCats = AppHelpers.UmbHelper().TypedContent(ids);
                foreach (var cat in allCats)
                {
                    cats.Add(CategoryMapper.MapCategory(cat));
                }
            }
            return cats;
        }

        /// <summary>
        /// Return all categories
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Category> GetAll()
        {
            const string cacheKey = "GetAllCategories";
            return (IEnumerable<Category>)ApplicationContext.Current.ApplicationCache.RequestCache.GetCacheItem(cacheKey, () =>
            {
                return CategoryMapper.MapCategory(_forumRootNode.Descendants(AppConstants.DocTypeForumCategory).ToList());
            });
        }

        /// <summary>
        /// Return all sub categories from a parent category id
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public IEnumerable<Category> GetAllSubCategories(Guid parentId)
        {
            return CategoryMapper.MapCategory(_forumRootNode.Descendants(AppConstants.DocTypeForumCategory)
                                                .Where(x => x.Parent.DocumentTypeAlias != AppConstants.DocTypeForumCategory)
                                                .ToList());
        }

        /// <summary>
        /// Get all main categories (Categories with no parent category)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Category> GetAllMainCategories()
        {
            return CategoryMapper.MapCategory(_forumRootNode.Descendants(AppConstants.DocTypeForumCategory)
                                                .Where(x => x.Parent.DocumentTypeAlias != AppConstants.DocTypeForumCategory).ToList());


        }

        public List<Category> GetAllowedCategories(IMemberGroup role, PermissionService permissionService, MemberService memberService, CategoryPermissionService categoryPermissionService)
        {
            return GetAllowedCategories(role, AppConstants.PermissionDenyAccess, permissionService, memberService, categoryPermissionService);
        }


        public List<Category> GetAllowedCategories(IMemberGroup role, string actionType, PermissionService permissionService, MemberService memberService, CategoryPermissionService categoryPermissionService)
        {
            var cacheKey = string.Concat("GetAllowedCategoriesCode-", role.Id, "-", actionType);
            return (List<Category>)ApplicationContext.Current.ApplicationCache.RequestCache.GetCacheItem(cacheKey, () =>
            {
                var filteredCats = new List<Category>();
                var allCats = GetAll();
                foreach (var category in allCats)
                {
                    var permissionSet = permissionService.GetPermissions(category, role, memberService, categoryPermissionService);
                    if (!permissionSet[actionType].IsTicked)
                    {
                        // TODO Only add it category is NOT locked
                        filteredCats.Add(category);
                    }
                }
                return filteredCats;
            });          
        }

        /// <summary>
        /// The allowed Categories of a member
        /// </summary>
        /// <param name="memberGroup"></param>
        /// <param name="permissionService"></param>
        /// <param name="memberService"></param>
        /// <param name="categoryPermissionService"></param>
        /// <returns></returns>
        public List<Category> AllowedCreateCategories(IMemberGroup memberGroup, PermissionService permissionService, MemberService memberService, CategoryPermissionService categoryPermissionService)
        {
            var allowedAccessCategories = GetAllowedCategories(memberGroup, permissionService, memberService, categoryPermissionService);
            var allowedCreateTopicCategories = GetAllowedCategories(memberGroup, AppConstants.PermissionCreateTopics, permissionService, memberService, categoryPermissionService);
            var allowedCreateTopicCategoryIds = allowedCreateTopicCategories.Select(x => x.Id);
            if (allowedAccessCategories.Any())
            {
                allowedAccessCategories.RemoveAll(x => allowedCreateTopicCategoryIds.Contains(x.Id));
                allowedAccessCategories.RemoveAll(x => memberGroup.Name != AppConstants.AdminRoleName && x.LockCategory);
            }
            return allowedAccessCategories;
        }

        ///// <summary>
        ///// Return allowed categories based on the users role
        ///// </summary>
        ///// <param name="role"></param>
        ///// <param name="permissionService"></param>
        ///// <param name="memberService"></param>
        ///// <param name="categoryPermissionService"></param>
        ///// <returns></returns>
        //public IEnumerable<Category> GetAllowedCategories(IMemberGroup role, PermissionService permissionService, MemberService memberService, CategoryPermissionService categoryPermissionService)
        //{
        //    var cacheKey = 
        //    return ApplicationContext.Current.ApplicationCache.RequestCache.GetCacheItem("", () =>
        //    {

        //    });
        //    var filteredCats = new List<Category>();
        //    var allCats = GetAll().Where(x => !x.LockCategory);
        //    foreach (var category in allCats)
        //    {
        //        var permissionSet = permissionService.GetPermissions(category, role, memberService, categoryPermissionService);
        //        if (!permissionSet[AppConstants.PermissionDenyAccess].IsTicked && !permissionSet[AppConstants.PermissionReadOnly].IsTicked)
        //        {
        //            filteredCats.Add(category);
        //        }
        //    }
        //    return filteredCats;
        //}

    }
}