using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Dialogue.Logic.Application.ExtensionMethods
{
    public static class AppExtensionMethods
    {

        public static string KiloFormat(this int num)
        {
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.#") + "M";

            if (num >= 10000)
                return (num / 1000D).ToString("#,0K");

            if (num >= 1000)
                return (num / 1000D).ToString("0.#") + "K";

            return num.ToString(CultureInfo.InvariantCulture);
        } 

        public static MvcHtmlString Pager(this HtmlHelper helper, int currentPage, int pageSize, int totalItemCount, object routeValues)
        {
            // how many pages to display in each page group const  	
            const int cGroupSize = AppConstants.PagingGroupSize;
            var thisPageUrl = HttpContext.Current.Request.Url.AbsolutePath;
            var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            if (pageCount <= 0)
            {
                return null;
            }

            // cleanup any out bounds page number passed  	
            currentPage = Math.Max(currentPage, 1);
            currentPage = Math.Min(currentPage, pageCount);

            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var containerdiv = new TagBuilder("div");
            containerdiv.AddCssClass("paginationholder");
            var container = new TagBuilder("ul");
            container.AddCssClass("pagination");

            // calculate the last page group number starting from the current page  	
            // until we hit the next whole divisible number  	
            var lastGroupNumber = currentPage;
            while ((lastGroupNumber % cGroupSize != 0)) lastGroupNumber++;

            // correct if we went over the number of pages  	
            var groupEnd = Math.Min(lastGroupNumber, pageCount);

            // work out the first page group number, we use the lastGroupNumber instead of  	
            // groupEnd so that we don't include numbers from the previous group if we went  	
            // over the page count  	
            var groupStart = lastGroupNumber - (cGroupSize - 1);

            // if we are past the first page  	
            if (currentPage > 1)
            {
                var previousli = new TagBuilder("li");
                var previous = new TagBuilder("a");
                previous.SetInnerText("<");
                previous.AddCssClass("previous");
                previous.MergeAttribute("href", String.Format("{0}?{1}", thisPageUrl, String.Concat("p=", currentPage - 1)));
                previousli.InnerHtml = previous.ToString();
                container.InnerHtml += previousli;
            }

            // if we have past the first page group  	
            if (currentPage > cGroupSize)
            {
                var previousDotsli = new TagBuilder("li");
                var previousDots = new TagBuilder("a");
                previousDots.SetInnerText("...");
                previousDots.AddCssClass("previous-dots");
                previousDots.MergeAttribute("href", String.Format("{0}?{1}", thisPageUrl, String.Concat("p=", groupStart - cGroupSize)));
                previousDotsli.InnerHtml = previousDots.ToString();
                container.InnerHtml += previousDotsli.ToString();
            }

            for (var i = groupStart; i <= groupEnd; i++)
            {
                var pageNumberli = new TagBuilder("li");
                pageNumberli.AddCssClass(((i == currentPage)) ? "active" : "p");
                var pageNumber = new TagBuilder("a");
                pageNumber.SetInnerText((i).ToString());
                pageNumber.MergeAttribute("href", String.Format("{0}?{1}", thisPageUrl, String.Concat("p=", i)));
                pageNumberli.InnerHtml = pageNumber.ToString();
                container.InnerHtml += pageNumberli.ToString();
            }

            // if there are still pages past the end of this page group  	
            if (pageCount > groupEnd)
            {
                var nextDotsli = new TagBuilder("li");
                var nextDots = new TagBuilder("a");
                nextDots.SetInnerText("...");
                nextDots.AddCssClass("next-dots");
                nextDots.MergeAttribute("href", String.Format("{0}?{1}", thisPageUrl, String.Concat("p=", groupEnd + 1)));
                nextDotsli.InnerHtml = nextDots.ToString();
                container.InnerHtml += nextDotsli.ToString();
            }

            // if we still have pages left to show  	
            if (currentPage < pageCount)
            {
                var nextli = new TagBuilder("li");
                var next = new TagBuilder("a");
                next.SetInnerText(">");
                next.AddCssClass("next");
                next.MergeAttribute("href", String.Format("{0}?{1}", thisPageUrl, String.Concat("p=", currentPage + 1)));
                nextli.InnerHtml = next.ToString();
                container.InnerHtml += nextli.ToString();
            }
            containerdiv.InnerHtml = container.ToString();
            return MvcHtmlString.Create(containerdiv.ToString());
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }        
        }

        public static Models.Member CurrentMember(this HtmlHelper html)
        {
            return ServiceFactory.MemberService.CurrentMember();
        }
        public static IHtmlString ThemedPartial(this HtmlHelper html, string partialName, ViewDataDictionary viewData = null)
        {
            var path = PathHelper.GetThemePartialViewPath(partialName);
            return html.Partial(path, viewData);
        }
        public static IHtmlString ThemedPartial(this HtmlHelper html, string partialName, object viewModel)
        {
            var path = PathHelper.GetThemePartialViewPath(partialName);
            return html.Partial(path, viewModel);
        }
        public static string Lang(this HtmlHelper html, string key)
        {
            return AppHelpers.Lang(key);
        }

        public static DialogueSettings Settings(this HtmlHelper html)
        {
            return Dialogue.Settings();
        }

        public static bool IsVisibleOnSitemap(this IPublishedContent content)
        {
            return !content.HasValue(AppConstants.PropHideFromSiteMap) || !content.GetPropertyValue<bool>(AppConstants.PropHideFromSiteMap);
        }

        /// <summary>
        /// POCO auto mapper for Published content
        /// example Model.Content.As<MyCustomModel>()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static T As<T>(this IPublishedContent content)
        {
            // Create an empty instance of the POCO
            var poco = Activator.CreateInstance<T>();

            // Discover properties of the poco with reflection
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var pocoType = poco.GetType();

            foreach (var propertyInfo in properties)
            {
                var contentType = content.GetType();
                if (content.GetType().GetProperty(propertyInfo.Name) != null)
                {
                    // It is a default propery - get the value with refelection
                    var propertyValue = contentType.GetProperty(propertyInfo.Name).GetValue(content, null);
                    pocoType.GetProperty(propertyInfo.Name).SetValue(poco, propertyValue, null);
                }
                else
                {
                    // it is a doctype property - ask Umbraco for the value
                    var propertyValue = content.GetPropertyValue(propertyInfo.Name);
                    pocoType.GetProperty(propertyInfo.Name).SetValue(poco, propertyValue, null);
                }
            }

            return poco;
        }
    }
}