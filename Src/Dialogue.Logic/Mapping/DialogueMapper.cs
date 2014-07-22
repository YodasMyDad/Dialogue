using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Umbraco.Core.Models;
using Umbraco.Web;
using umbraco;

namespace Dialogue.Logic.Mapping
{
    public static class DialogueMapper
    {
        public static DialogueForum MapRootForum(IPublishedContent model)
        {
            var pageModel = new DialogueForum
                {
                    MainHeading = PageHeading(model),
                    MainContent = MainContent(model),
                    MainCategories = new List<Category>()
                };

            var mainCategories = model.Children.Where(x => x.DocumentTypeAlias == AppConstants.DocTypeForumCategory).ToList();
            if (mainCategories.Any())
            {
                foreach (var publishedContent in mainCategories)
                {
                    pageModel.MainCategories.Add(CategoryMapper.MapCategory(publishedContent, true));
                }
            }

            // If this node has common properties then populate them
            // I.e. SEO & Umbraco Properties
            PopulateCommonUmbracoProperties(pageModel, model);

            return pageModel;
        }

        #region Misc

        public static string PageHeading(IPublishedContent content)
        {
            var heading = content.GetPropertyValue<string>(AppConstants.PropMainHeading);
            if (string.IsNullOrEmpty(heading))
            {
                heading = content.Name;
            }
            return heading;
        }

        public static string MainContent(IPublishedContent content)
        {
            var bodyText = content.GetPropertyValue<string>(AppConstants.PropBodyText);
            if (string.IsNullOrEmpty(bodyText))
            {
                // Trying mainContent
                bodyText = content.GetPropertyValue<string>("mainContent");
            }
            return bodyText;
        }

        /// <summary>
        /// Common properties to map
        /// </summary>
        /// <param name="modelObject"></param>
        /// <param name="model"></param>
        public static void PopulateCommonUmbracoProperties(MasterModel modelObject, IPublishedContent model)
        {
            modelObject.PageTitle = model.HasValue(AppConstants.PropPageTitle) ? model.GetPropertyValue<string>(AppConstants.PropPageTitle) : model.Name;
            modelObject.MetaDesc = model.GetPropertyValue(AppConstants.PropMetaDesc, library.TruncateString(library.StripHtml(model.GetPropertyValue<string>(AppConstants.PropBodyText, "")), 160, "")).ToString();
            modelObject.NodePath = model.Path.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            modelObject.HideFromNavigation = model.GetPropertyValue<bool>(AppConstants.PropUmbracoNaviHide);
            modelObject.ShowInFooter = model.GetPropertyValue<bool>(AppConstants.PropShowInFooter);
            modelObject.ConversionCode = model.GetPropertyValue<string>(AppConstants.PropConversionCodes);
        }

        #endregion
    }
}