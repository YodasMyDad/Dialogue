using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Dialogue.Logic.Mapping
{
    public static class CategoryMapper
    {
        #region Category Notifications

        /// <summary>
        /// Maps full members and categories into the model
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public static List<CategoryNotification> CompleteModels(List<CategoryNotification> entityList)
        {
            // Map full Members
            var membersIds = entityList.Select(x => x.MemberId).ToList();
            var members = MemberMapper.MapMember(membersIds);
            foreach (var entity in entityList)
            {
                var member = members.FirstOrDefault(x => x.Id == entity.MemberId);
                entity.Member = member;
            }

            // Map full categories
            var catIds = entityList.Select(x => x.CategoryId).ToList();
            var cats = MapCategory(catIds);
            foreach (var entity in entityList)
            {
                var cat = cats.FirstOrDefault(x => x.Id == entity.CategoryId);
                entity.Category = cat;
            }

            return entityList;
        }

        #endregion

        #region Category

        public static CategoryViewModel ToViewModel(this Category cat)
        {

            var viewModel = new CategoryViewModel
            {
                Description = cat.Description,
                Id = cat.Id,
                Image = cat.Image,
                Name = cat.Name
            };

            return viewModel;
        }

        /// <summary>
        /// Maps a category, cached per request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="getSubAndParentCats"></param>
        /// <returns></returns>
        public static Category MapCategory(IPublishedContent model, bool getSubAndParentCats = false)
        {

            if (model != null)
            {
                var key = string.Format("umb-cat{0}-{1}", model.Id, getSubAndParentCats);
                if (!HttpContext.Current.Items.Contains(key))
                {

                    var pageModel = new Category(model);
                    pageModel.Description = model.GetPropertyValue<string>("description");
                    pageModel.Image = AppHelpers.GetMediaUrlFromProperty(model, "categoryImage");
                    pageModel.LockCategory = model.GetPropertyValue<bool>("lockCategory");
                    pageModel.ModerateAllTopicsInThisCategory = model.GetPropertyValue<bool>("moderateAllTopicsInThisCategory");
                    pageModel.ModerateAllPostsInThisCategory = model.GetPropertyValue<bool>("moderateAllPostsInThisCategory");
                    pageModel.SubCategories = new List<Category>();
                    pageModel.ParentCategories = new List<Category>();

                    // If this node has common properties then populate them
                    // I.e. SEO & Umbraco Properties
                    DialogueMapper.PopulateCommonUmbracoProperties(pageModel, model);

                    // Only get subcategories if the user has requested it
                    if (getSubAndParentCats)
                    {
                        var subCategories = model.Children.ToList();
                        if (model.Children.Any())
                        {
                            foreach (var publishedContent in subCategories)
                            {
                                pageModel.SubCategories.Add(MapCategory(publishedContent, true));
                            }
                        }

                        var path = model.Path;
                        if (!string.IsNullOrEmpty(path))
                        {
                            var catIds = path.Trim().Split(',').Select(x => Convert.ToInt32(x)).ToList();
                            catIds.Remove(model.Id);
                            var allNodes =
                                AppHelpers.UmbHelper()
                                    .TypedContent(catIds)
                                    .Where(x => x != null && x.DocumentTypeAlias == AppConstants.DocTypeForumCategory);
                            foreach (var cat in allNodes)
                            {
                                pageModel.ParentCategories.Add(MapCategory(cat));
                            }
                        }
                    }

                    HttpContext.Current.Items.Add(key, pageModel);
                }

                return HttpContext.Current.Items[key] as Category;
            }
            return null;
        }

        public static List<Category> MapCategory(List<IPublishedContent> cats)
        {
            var mappedCats = new List<Category>();
            foreach (var cat in cats)
            {
                mappedCats.Add(MapCategory(cat));
            }
            return mappedCats;
        }

        public static List<Category> MapCategory(List<int> catids)
        {
            var mappedCats = new List<Category>();
            foreach (var cat in catids)
            {
                mappedCats.Add(MapCategory(cat));
            }
            return mappedCats;
        }

        public static Category MapCategory(int id)
        {
            var cat = AppHelpers.GetNode(id);
            return MapCategory(cat);
        }
        #endregion

    }
}