﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Dialogue.Logic.Services
{
    public partial class CategoryService
    {
        private readonly IPublishedContent _forumRootNode;

        public CategoryService()
        {
            _forumRootNode = AppHelpers.GetNode(Dialogue.Settings().ForumId);
        }
        public Category Get(int id)
        {
            return CategoryMapper.MapCategory(AppHelpers.GetNode(id));
        }

        public List<Category> Get(List<int> ids)
        {
            var cats = new List<Category>();
            if (ids.Any())
            {
                foreach (var catId in ids)
                {
                    cats.Add(CategoryMapper.MapCategory(AppHelpers.GetNode(catId)));
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
            return CategoryMapper.MapCategory(_forumRootNode.Descendants(AppConstants.DocTypeForumCategory).ToList());
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
                                                .Where(x => !x.Children.Any()).ToList());


        }

        /// <summary>
        /// Return allowed categories based on the users role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public IEnumerable<Category> GetAllowedCategories(IMemberGroup role)
        {
            var filteredCats = new List<Category>();
            var allCats = GetAll();
            foreach (var category in allCats)
            {
                var permissionSet = ServiceFactory.PermissionService.GetPermissions(category, role);
                if (!permissionSet[AppConstants.PermissionDenyAccess].IsTicked)
                {
                    filteredCats.Add(category);
                }
            }
            return filteredCats;
        }

    }
}