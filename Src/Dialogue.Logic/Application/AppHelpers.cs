using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Services;
using HtmlAgilityPack;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Security;
using umbraco;

namespace Dialogue.Logic.Application
{
    public static class AppHelpers
    {

        #region Social

        public static string md5HashString(string toHash)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(toHash));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();  // Return the hexadecimal string.
        }

        public static string MemberImage(string avatar, string email, int userId, int size)
        {
            if (!string.IsNullOrEmpty(avatar))
            {
                // Has an avatar image
                return VirtualPathUtility.ToAbsolute(string.Concat("~/", avatar, string.Format("?width={0}&crop=0,0,{0},{0}", size)));
            }
            return GetGravatarImage(email, size);
        }

        public static string GetGravatarImage(string email, int size)
        {
            return IsValidEmail(email) ? string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=identicon", md5HashString(email), size) : "";
        }

        #endregion

        #region Validation

        private static bool invalid = false;
        public static bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            var domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }

        #endregion

        #region SEO

        private const string CanonicalNext = "<link href=\"{0}\" rel=\"next\" />";
        private const string CanonicalPrev = "<link href=\"{0}\" rel=\"prev\" />";
        private const string Canonical = "<link href=\"{0}\" rel=\"canonical\" />";

        public static string CanonicalPagingTag(int totalItemCount, int pageSize, HtmlHelper helper)
        {
            var urlHelper = new System.Web.Mvc.UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var currentAction = helper.ViewContext.RouteData.GetRequiredString("Action");
            var url = urlHelper.Action(currentAction, new { });

            var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            var nextTag = String.Empty;
            var previousTag = String.Empty;

            var req = HttpContext.Current.Request["p"];
            var page = req != null ? Convert.ToInt32(req) : 1;

            // Sort the canonical tag out
            var canonicalTag = String.Format(Canonical, page <= 1 ? url : String.Format(AppConstants.PagingUrlFormat, url, page));

            // On the first page       
            if (pageCount > 1 & page <= 1)
            {
                nextTag = String.Format(CanonicalNext, String.Format(AppConstants.PagingUrlFormat, url, (page + 1)));
            }

            // On a page greater than the first page, but not the last
            if (pageCount > 1 & page > 1 & page < pageCount)
            {
                nextTag = String.Format(CanonicalNext, String.Format(AppConstants.PagingUrlFormat, url, (page + 1)));
                previousTag = String.Format(CanonicalPrev, String.Format(AppConstants.PagingUrlFormat, url, (page - 1)));
            }

            // On the last page
            if (pageCount > 1 & pageCount == page)
            {
                previousTag = String.Format(CanonicalPrev, String.Format(AppConstants.PagingUrlFormat, url, (page - 1)));
            }

            // return the canoncal tags
            return String.Concat(canonicalTag, Environment.NewLine,
                                    nextTag, Environment.NewLine,
                                    previousTag);
        }

        #endregion

        #region General Umbraco

        public static Language GetRootLanguage(IPublishedContent currentNode)
        {
            var key = string.Concat("lang-page-", currentNode.Id);
            if (!HttpContext.Current.Items.Contains(key))
            {
                var domains = Domain.GetDomainsById(currentNode.AncestorOrSelf(1).Id);
                if (domains != null && domains.Any())
                {
                    HttpContext.Current.Items.Add(key, domains[0].Language);                    
                }
                else
                {
                    return null;
                }
            }
            return HttpContext.Current.Items[key] as Language;            
        }

        public static ILanguage GetLanguage(string languageCode)
        {
            var key = string.Concat("lang-", languageCode);
            if (!HttpContext.Current.Items.Contains(key))
            {
                HttpContext.Current.Items.Add(key, UmbServices().LocalizationService.GetLanguageByCultureCode(languageCode));
            }
            return HttpContext.Current.Items[key] as ILanguage;
        }

        public static string Lang(string key)
        {
            
            var cachekey = string.Concat("dialoguedictionary-", key);
            if (!HttpContext.Current.Items.Contains(cachekey))
            {
                var dictResult = key;
                if (!string.IsNullOrEmpty(key))
                {
                    try
                    {
                        // Get the dictionary item
                        var dictValue = UmbHelper().GetDictionaryValue(key);
                        if (!string.IsNullOrEmpty(dictValue))
                        {
                            dictResult = dictValue;
                        }

                        if (dictResult == key)
                        {
                            // Fall back...
                            var result = library.GetDictionaryItem(key.Trim());
                            if (!string.IsNullOrEmpty(result))
                            {
                                dictResult = result;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Just return original key
                    }

                }
                else
                {
                    dictResult = "Error, no dictionary key";
                }

                HttpContext.Current.Items.Add(cachekey, dictResult);
            }
            return HttpContext.Current.Items[cachekey] as string;
        }

        //public static string Lang(string key, string languageCode)
        //{            
        //var cultureCode = Thread.CurrentThread.CurrentCulture.Name;
        //    var cachekey = string.Concat("dict-", key, "-", languageCode);
        //    if (!HttpContext.Current.Items.Contains(cachekey))
        //    {
        //        var dictResult = key;
        //        if (!string.IsNullOrEmpty(key))
        //        {
        //            try
        //            {
        //                // Get the dictionary item
        //                var dictionaryItem = UmbServices().LocalizationService.GetDictionaryItemByKey(key.Trim());

        //                // See if we have a language too
        //                if (string.IsNullOrEmpty(languageCode) && dictionaryItem != null)
        //                {
        //                    // We don't so just get the first dictionary value found for this key
        //                    var firstFound = dictionaryItem.Translations.FirstOrDefault();
        //                    if (firstFound != null)
        //                    {
        //                        dictResult = firstFound.Value;
        //                    }
        //                }
        //                else if (dictionaryItem != null)
        //                {
        //                    // We have a language, so get the correct dictionery item by the language ISO code
        //                    var dict = dictionaryItem.Translations.FirstOrDefault(x => x.Language.IsoCode == languageCode);
        //                    if (dict != null)
        //                    {
        //                        dictResult = dict.Value;
        //                    }                  
        //                }

        //                if (dictResult == key)
        //                {
        //                    // Fall back...
        //                    var result = library.GetDictionaryItem(key.Trim());
        //                    if (!string.IsNullOrEmpty(result))
        //                    {
        //                        dictResult = result;
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                // Just return original key
        //            }

        //        }
        //        else
        //        {
        //            dictResult = "Error, no dictionary key";
        //        }

        //        HttpContext.Current.Items.Add(cachekey, dictResult);
        //    }
        //    return HttpContext.Current.Items[cachekey] as string;


        //}


        /// <summary>
        /// Get access to the Umbraco helper
        /// </summary>
        /// <returns></returns>
        public static UmbracoHelper UmbHelper()
        {
            const string key = "umb-helper";
            if (!HttpContext.Current.Items.Contains(key))
            {
                HttpContext.Current.Items.Add(key, new UmbracoHelper(UmbracoContext.Current));
            }
            return HttpContext.Current.Items[key] as UmbracoHelper;
        }

        /// <summary>
        /// Get the member helper
        /// </summary>
        /// <returns></returns>
        public static MembershipHelper UmbMemberHelper()
        {
            const string key = "umb-memberhelper";
            if (!HttpContext.Current.Items.Contains(key))
            {
                HttpContext.Current.Items.Add(key, new MembershipHelper(UmbracoContext.Current));
            }
            return HttpContext.Current.Items[key] as MembershipHelper;
        }

        /// <summary>
        /// Get the Umbraco services
        /// </summary>
        /// <returns></returns>
        public static ServiceContext UmbServices()
        {
            return ApplicationContext.Current.Services;
        }

        /// <summary>
        /// Get a cached per request list of all member groups
        /// </summary>
        /// <returns></returns>
        public static List<IMemberGroup> GetAllMemberGroups()
        {
            const string key = "umb-allmembergroups";
            if (!HttpContext.Current.Items.Contains(key))
            {
                var memberGroups = UmbServices().MemberGroupService.GetAll().ToList();
                HttpContext.Current.Items.Add(key, memberGroups);
            }
            return HttpContext.Current.Items[key] as List<IMemberGroup>;
        }

        /// <summary>
        /// Logs an error but opn as info as Umbraco error logging is weird now
        /// </summary>
        /// <param name="error"></param>
        public static void LogError(string error)
        {
            error = string.Format("Dialogue Package: {0}", error);
            LogHelper.Warn<IHtmlString>(error);
        }
        public static void LogError(string error, Exception ex)
        {
            error = string.Format("Dialogue Package: {0}", error);
            LogHelper.Error<IHtmlString>(error, ex);
        }
        public static void LogError(Exception ex)
        {
            LogHelper.Error<IHtmlString>("Dialogue error", ex);
        }

        /// <summary>
        /// Returns Multi Node Tree Picker Id's from a CSV string
        /// </summary>
        /// <param name="mntpIds"></param>
        /// <returns></returns>
        public static IEnumerable<int> ReturnMntpIds(string mntpIds)
        {
            if (!string.IsNullOrEmpty(mntpIds))
            {
                int parsedWidgetId;
                var widgetIds = mntpIds
                    .Split(',').Select(x => int.TryParse(x, out parsedWidgetId) ? parsedWidgetId : 0)
                    .Where(x => x != 0);

                return widgetIds;
            }
            return null;
        }

        #endregion

        #region Media

        public static Image GetImageFromExternalUrl(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            using (var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var stream = httpWebReponse.GetResponseStream())
                {
                    if (stream != null) return Image.FromStream(stream);
                }
            }
            return null;
        }

        public static string GetMemberUploadPath(int memberId)
        {
            var uploadFolderPath = HttpContext.Current.Server.MapPath(string.Concat(AppConstants.UploadFolderPath, memberId));
            if (!Directory.Exists(uploadFolderPath))
            {
                Directory.CreateDirectory(uploadFolderPath);
            }
            return uploadFolderPath;
        }

        public static UploadFileResult UploadFile(HttpPostedFileBase file, string uploadFolderPath, bool onlyImages = false)
        {
            var upResult = new UploadFileResult { UploadSuccessful = true };

            var fileName = Path.GetFileName(file.FileName);
            if (fileName != null)
            {

                upResult = FileChecks(file, upResult, fileName, onlyImages);
                if (!upResult.UploadSuccessful)
                {
                    return upResult;
                }

                // Sort the file name
                var newFileName = string.Format("{0}_{1}", GenerateComb(), fileName.Trim(' ').Replace("_", "-").Replace(" ", "-").ToLower());
                var path = Path.Combine(uploadFolderPath, newFileName);

                // Save the file to disk
                file.SaveAs(path);

                var hostingRoot = HostingEnvironment.MapPath("~/") ?? "";
                var fileUrl = path.Substring(hostingRoot.Length).Replace('\\', '/').Insert(0, "/");

                upResult.UploadedFileName = newFileName;
                upResult.UploadedFileUrl = fileUrl;
            }

            return upResult;
        }

        /// <summary>
        /// Upload a file to an Umbraco upload field
        /// </summary>
        /// <param name="file"></param>
        /// <param name="onlyImages"></param>
        /// <returns></returns>
        public static UploadFileResult UploadFile(HttpPostedFileBase file, bool onlyImages = false)
        {
            var upResult = new UploadFileResult { UploadSuccessful = true };

            var fileName = Path.GetFileName(file.FileName);
            if (fileName != null)
            {
                // check standard Dialogue stuff
                upResult = FileChecks(file, upResult, fileName, onlyImages);
                if (!upResult.UploadSuccessful)
                {
                    return upResult;
                }

                // Create the umbraco media file
                var memberMediaFolderId = ConfirmMemberAvatarMediaFolder();
                var ms = UmbServices().MediaService;
                var media = ms.CreateMedia(fileName, memberMediaFolderId, "Image");
                media.SetValue("umbracoFile", file);
                ms.Save(media);

                // Get this saved media out the cache
                var typedMedia = UmbHelper().TypedMedia(media.Id);

                // Set the Urls
                upResult.UploadedFileName = typedMedia.Name;
                upResult.UploadedFileUrl = typedMedia.Url;
                upResult.MediaId = typedMedia.Id;
            }

            return upResult;
        }

        private static UploadFileResult FileChecks(HttpPostedFileBase file, UploadFileResult upResult, string fileName, bool onlyImages = false)
        {
            //Before we do anything, check file size
            if (file.ContentLength > Dialogue.Settings().FileUploadMaximumFilesize)
            {
                //File is too big
                upResult.UploadSuccessful = false;
                upResult.ErrorMessage = Lang("Post.UploadFileTooBig");
                return upResult;
            }

            // now check allowed extensions
            var allowedFileExtensions = Dialogue.Settings().FileUploadAllowedExtensions;

            if (onlyImages)
            {
                allowedFileExtensions = new List<string>
                    {
                        "jpg",
                        "jpeg",
                        "png",
                        "gif"
                    };
            }

            if (allowedFileExtensions.Any())
            {

                // Get the file extension
                var fileExtension = Path.GetExtension(fileName.ToLower());

                // If can't work out extension then just error
                if (string.IsNullOrEmpty(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = Lang("Errors.GenericMessage");
                    return upResult;
                }

                // Remove the dot then check against the extensions in the web.config settings
                fileExtension = fileExtension.TrimStart('.');
                if (!allowedFileExtensions.Contains(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = Lang("Post.UploadBannedFileExtension");
                    return upResult;
                }
            }

            return upResult;
        }

        public static int ConfirmMemberAvatarMediaFolder()
        {
            // We want to look for the 'Member Avatars' folder in the media section
            // If it doesn't exist then we create it
            var rootMediaId = -1;
            const string folderName = "Dialogue Members Avatars";

            // Media Service
            var ms = UmbServices().MediaService;
            
            // Check main media folder
            var mediaFolder = ms.GetRootMedia().FirstOrDefault(x => x.Name == folderName);
            if (mediaFolder == null)
            {
                // Doesn't exist, so create it
                mediaFolder = ms.CreateMedia(folderName, rootMediaId, "Folder");
                ms.Save(mediaFolder);
            }

            // Set id
            rootMediaId = mediaFolder.Id;

            // Check current user has a folder
            var cUser = ServiceFactory.MemberService.CurrentMember();
            if (cUser != null)
            {
                // Get the members folder
                var memberFolder = mediaFolder.Children().FirstOrDefault(x => x.Name == cUser.UserName);
                if (memberFolder == null)
                {
                    // Doesn't exist, so create it
                    memberFolder = ms.CreateMedia(cUser.UserName, mediaFolder.Id, "Folder");
                    ms.Save(memberFolder);
                }

                // reset id
                rootMediaId = memberFolder.Id;
            }

            return rootMediaId;
        }

        public static UploadFileResult UploadFile(Image file, string uploadFolderPath)
        {

            var upResult = new UploadFileResult { UploadSuccessful = true };
            var fileName = string.Concat(GenerateComb(), ".jpg").ToLower();

            // now check allowed extensions
            var allowedFileExtensions = Dialogue.Settings().FileUploadAllowedExtensions;

            if (allowedFileExtensions.Any())
            {

                // Get the file extension
                var fileExtension = Path.GetExtension(fileName.ToLower());

                // If can't work out extension then just error
                if (string.IsNullOrEmpty(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = Lang("Errors.GenericMessage");
                    return upResult;
                }

                // Remove the dot then check against the extensions in the web.config settings
                fileExtension = fileExtension.TrimStart('.');
                if (!allowedFileExtensions.Contains(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = Lang("Post.UploadBannedFileExtension");
                    return upResult;
                }
            }

            // Sort the file name
            var path = Path.Combine(uploadFolderPath, fileName);

            // Save the file to disk
            file.Save(path, ImageFormat.Jpeg);

            var hostingRoot = HostingEnvironment.MapPath("~/") ?? "";
            var fileUrl = path.Substring(hostingRoot.Length).Replace('\\', '/').Insert(0, "/");

            upResult.UploadedFileName = fileName;
            upResult.UploadedFileUrl = fileUrl;
            return upResult;
        }

        public static string ReturnBadgeUrl(string badgeFile)
        {
            return string.Concat(AppConstants.AssetsImagePath, "badges/", badgeFile);
        }
        public static string GetStaticMediaUrl(string imagename)
        {
            return string.Concat(AppConstants.AssetsImagePath, imagename);
        }
        /// <summary>
        /// Returns a list of media urls from a csv string of media ids
        /// </summary>
        /// <param name="mediaCsv"></param>
        /// <returns></returns>
        public static IList<string> ReturnMediaUrls(string mediaCsv)
        {
            try
            {
                if (string.IsNullOrEmpty(mediaCsv))
                {
                    return null;
                }
                var mediaIds = ReturnMntpIds(mediaCsv);
                return mediaIds.Select(id => GetMediaNode(id).Url)
                                .Where(url => !string.IsNullOrEmpty(url))
                                .ToList();
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Get a crop for an image
        /// </summary>
        /// <param name="content"></param>
        /// <param name="cropPropertyName">Property name of image picker or media item</param>
        /// <param name="cropName">The crop Alias defined on the datatype</param>
        /// <returns></returns>
        public static string GetCrop(this IPublishedContent content, string cropPropertyName, string cropName)
        {
            try
            {
                var typedMedia = GetMediaNode(content.GetPropertyValue<int>(cropPropertyName));
                return typedMedia.GetCropUrl(cropName);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Get a list of images from CSV and return their crops by name
        /// </summary>
        /// <param name="content"></param>
        /// <param name="cropPropertyName"></param>
        /// <param name="cropName"></param>
        /// <returns></returns>
        public static IList<string> GetCrops(this IPublishedContent content, string cropPropertyName, string cropName)
        {
            try
            {
                var mediaIds = ReturnMntpIds(content.GetPropertyValue<string>(cropPropertyName));
                var mediaList = mediaIds.Select(GetMediaNode).ToList();
                var medicropUrls = new List<string>();
                foreach (var mediaItem in mediaList)
                {
                    medicropUrls.Add(mediaItem.GetCropUrl(cropName));
                }
                return medicropUrls;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Returns the Url of a media item from the property name
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetMediaUrlFromProperty(IPublishedContent content, string propertyName)
        {
            var mediaId = content.GetPropertyValue<string>(propertyName);
            if (!string.IsNullOrEmpty(mediaId))
            {
                IPublishedContent mediaItem;
                try
                {
                    mediaItem = UmbHelper().TypedMedia(mediaId);
                }
                catch (Exception)
                {
                    mediaItem = null;
                }
                if (mediaItem != null)
                {
                    return mediaItem.Url;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// Returns a media node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IPublishedContent GetMediaNode(int id)
        {
            try
            {
                var mediaItem = UmbHelper().TypedMedia(id);
                return mediaItem;
            }
            catch (Exception)
            {
                LogError(string.Format("Unable to find media with Id of {0}", id));
            }
            return null;
        }
        /// <summary>
        /// Returns a media node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IPublishedContent GetMediaNode(string id)
        {
            return id != null ? GetMediaNode(Convert.ToInt32(id)) : null;
        }
        #endregion

        #region Content

        /// <summary>
        /// Returns the current page
        /// </summary>
        /// <returns></returns>
        public static IPublishedContent CurrentPage()
        {
            var contentRequest = UmbracoContext.Current.PublishedContentRequest;
            if (contentRequest != null)
            {
                return contentRequest.PublishedContent;
            }

            // Not ideal - But if blank, return the first instance of a dialogue forum we can find
            return UmbracoContext.Current.ContentCache.GetByXPath(string.Concat("//", AppConstants.DocTypeForumRoot)).FirstOrDefault();
        }


        public static void SetCurrentPage(int nodeId)
        {
            UmbracoContext.Current.PublishedContentRequest.PublishedContent = GetNode(nodeId);
        }

        /// <summary>
        /// Get a node by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IPublishedContent GetNode(int id)
        {
            var key = string.Concat("node-", id);
            if (!HttpContext.Current.Items.Contains(key))
            {
                var node = UmbHelper().TypedContent(id);
                HttpContext.Current.Items.Add(key, node);
            }
            return HttpContext.Current.Items[key] as IPublishedContent;
        }

        /// <summary>
        /// Gets the url of a node by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetContentUrl(int? id)
        {
            if (id != null)
            {
                var node = GetNode(Convert.ToInt32(id));
                return node.Url;
            }
            return "/";
        }

        #endregion

        #region String Manipulation


        public static string ConvertPostContent(string post)
        {
            if (!string.IsNullOrEmpty(post))
            {

                // If using the PageDown/MarkDown Editor uncomment this line
                post = ConvertMarkDown(post);

                // Allow video embeds
                post = EmbedVideosInPosts(post);

                // Add Google prettify code snippets
                post = post.Replace("<pre>", "<pre class='prettyprint'>");
            }

            return post;
        }

        /// <summary>
        /// Converts markdown into HTML
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertMarkDown(string str)
        {
            var md = new MarkdownSharp.Markdown { AutoHyperlink = false, LinkEmails = false };
            return md.Transform(str);
        }

        public static string EmbedVideosInPosts(string str)
        {
            if (str.IndexOf("youtube.com", StringComparison.CurrentCultureIgnoreCase) > 0 || str.IndexOf("youtu.be", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
                const string replacement = "<div class=\"video-container\"><iframe title='YouTube video player' width='500' height='281' src='http://www.youtube.com/embed/$1' frameborder='0' allowfullscreen='1'></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("vimeo.com", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                const string pattern = @"(?:https?:\/\/)?vimeo\.com/(?:.*#|.*/videos/)?([0-9]+)";
                const string replacement = "<div class=\"video-container\"><iframe src=\"http://player.vimeo.com/video/$1?portrait=0\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("screenr.com", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                const string pattern = @"(?:https?:\/\/)?(?:www\.)screenr\.com/([a-zA-Z0-9]+)";
                const string replacement = "<div class=\"video-container\"><iframe src=\"http://www.screenr.com/embed/$1\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            return str;
        }

        /// <summary>
        /// Returns a string to do a related question/search lookup
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static string ReturnSearchString(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return searchTerm;
            }

            // Lower case
            searchTerm = SafePlainText(searchTerm.ToLower());

            // Firstly strip non alpha numeric charactors out
            searchTerm = Regex.Replace(searchTerm, @"[^\w\.@\- ]", "");

            // Now strip common words out and retun the final result
            return string.Join(" ", searchTerm.Split().Where(w => !CommonWords().Contains(w)).ToArray()).Trim();
        }

        /// <summary>
        /// Returns a list of the most common english words
        /// TODO: Need to put this in something so people can add other language lists of common words
        /// </summary>
        /// <returns></returns>
        public static IList<string> CommonWords()
        {
            return new List<string>
                {
                    "the", "be",  "to", 
                    "of",  
                    "and",
                    "a",
                    "in",   
                    "that",  
                    "have",
                    "i",
                    "it",   
                    "for",
                    "not",
                    "on",
                    "with",
                    "he",
                    "as",
                    "you",
                    "do",
                    "at",
                    "this",
                    "but",
                    "his",
                    "by",
                    "from",
                    "they",
                    "we",
                    "say",
                    "her",
                    "she",
                    "or",
                    "an",
                    "will",
                    "my",
                    "one",
                    "all",
                    "would",
                    "there",
                    "their",
                    "what",
                    "so",
                    "up",
                    "out",
                    "if",
                    "about",
                    "who",
                    "get",
                    "which",
                    "go",
                    "me",
                    "when",
                    "make",
                    "can",
                    "like",
                    "time",
                    "no",
                    "just",
                    "him",
                    "know",
                    "take",
                    "people",
                    "into",
                    "year",
                    "your",
                    "good",
                    "some",
                    "could",
                    "them",
                    "see",
                    "other",
                    "than",
                    "then",
                    "now",
                    "look",
                    "only",
                    "come",
                    "its",
                    "over",
                    "think",
                    "also",
                    "back",
                    "after",
                    "use",
                    "two",
                    "how",
                    "our",
                    "work",
                    "first",
                    "well",
                    "way",
                    "even",
                    "new",
                    "want",
                    "because",
                    "any",
                    "these",
                    "give",
                    "day",
                    "most",
                    "cant",
                    "us"
                };
        }

        public static string GenerateSlug(string stringToSlug, IEnumerable<string> similarList, string previousSlug)
        {
            // url generator
            var slug = CreateUrl(stringToSlug, "-");

            // To list the entities
            var matchingEntities = similarList.ToList();
            if (matchingEntities.Any())
            {
                matchingEntities = matchingEntities.Where(x => !string.IsNullOrEmpty(x)).ToList();
            }
            else
            {
                return slug;
            }


            // If there is a previous slug, remove it from the similarList
            // basically remove itself from the list
            if (!string.IsNullOrEmpty(previousSlug))
            {
                matchingEntities.Remove(previousSlug);
            }

            // If there are no entities now, just return the slug
            if (matchingEntities.Count <= 0)
            {
                return slug;
            }


            // Entity Count
            var countToAdd = matchingEntities.Count();

            // Append the count to the current slug
            slug = string.Concat(slug, "-", countToAdd);


            return slug;
        }

        /// <summary>
        /// Creates a URL freindly string, good for SEO
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string CreateUrl(string strInput, string replaceWith)
        {
            // Doing this to stop the urls getting encoded
            var url = RemoveAccents(strInput);
            return StripNonAlphaNumeric(url, replaceWith).ToLower();
        }

        public static string CreateUrl(string strInput)
        {
            return CreateUrl(strInput, "-");
        }

        public static string RemoveAccents(string input)
        {
            // Replace accented characters for the closest ones:
            //var from = "ÂÃÄÀÁÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝàáâãäåçèéêëìíîïðñòóôõöøùúûüýÿ".ToCharArray();
            //var to = "AAAAAACEEEEIIIIDNOOOOOOUUUUYaaaaaaceeeeiiiidnoooooouuuuyy".ToCharArray();
            //for (var i = 0; i < from.Length; i++)
            //{
            //    input = input.Replace(from[i], to[i]);
            //}

            //// Thorn http://en.wikipedia.org/wiki/%C3%9E
            //input = input.Replace("Þ", "TH");
            //input = input.Replace("þ", "th");

            //// Eszett http://en.wikipedia.org/wiki/%C3%9F
            //input = input.Replace("ß", "ss");

            //// AE http://en.wikipedia.org/wiki/%C3%86
            //input = input.Replace("Æ", "AE");
            //input = input.Replace("æ", "ae");

            //return input;


            var stFormD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var t in stFormD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(t);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(t);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));

        }

        /// <summary>
        /// Strips all non alpha/numeric charators from a string
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string StripNonAlphaNumeric(string strInput, string replaceWith)
        {
            strInput = Regex.Replace(strInput, "[^\\w]", replaceWith);
            strInput = strInput.Replace(string.Concat(replaceWith, replaceWith, replaceWith), replaceWith)
                                .Replace(string.Concat(replaceWith, replaceWith), replaceWith)
                                .TrimStart(Convert.ToChar(replaceWith))
                                .TrimEnd(Convert.ToChar(replaceWith));
            return strInput;
        }

        public static string SafeEncodeUrlSegments(this string urlPath)
        {
            return string.Join("/",
                urlPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => HttpUtility.UrlEncode(x).Replace("+", "%20"))
                    .WhereNotNull()
                //we are not supporting dots in our URLs it's just too difficult to
                // support across the board with all the different config options
                    .Select(x => x.Replace('.', '-')));
        }

        #endregion

        #region HTML Manipulation

        public static string ReturnAmountWordsFromString(string text, int wordAmount)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string tmpStr;
            string[] stringArray;
            var tmpStrReturn = "";
            tmpStr = text.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");

            while (tmpStr.IndexOf("  ") != -1)
            {
                tmpStr = tmpStr.Replace("  ", " ");
            }
            stringArray = tmpStr.Split(' ');

            if (stringArray.Length < wordAmount)
            {
                wordAmount = stringArray.Length;
            }
            for (int i = 0; i < wordAmount; i++)
            {
                tmpStrReturn += stringArray[i] + " ";
            }
            return tmpStrReturn;
        }

        /// <summary>
        /// Uses regex to strip HTML from a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHtmlFromString(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = Regex.Replace(input, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", string.Empty, RegexOptions.Singleline);
                input = Regex.Replace(input, @"\[[^]]+\]", "");
            }
            return input;
        }

        /// <summary>
        /// Returns safe plain text using XSS library
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SafePlainText(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = GetSafeHtml(input);
                input = StripHtmlFromString(input);
            }
            return input;
        }

        /// <summary>
        /// Used to pass all string input in the system  - Strips all nasties from a string/html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetSafeHtml(string html)
        {
            // remove unwanted html
            html = RemoveUnwantedTags(html);

            // Scrub html
            html = ScrubHtml(html);

            return html;
        }


        /// <summary>
        /// Takes in HTML and returns santized Html/string
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ScrubHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //Remove potentially harmful elements
            var nc = doc.DocumentNode.SelectNodes("//script|//link|//iframe|//frameset|//frame|//applet|//object|//embed");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.ParentNode.RemoveChild(node, false);

                }
            }

            //remove hrefs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
            if (nc != null)
            {

                foreach (var node in nc)
                {
                    node.SetAttributeValue("href", "#");
                }
            }

            //remove img with refs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.SetAttributeValue("src", "#");
                }
            }

            //remove on<Event> handlers from all tags
            nc = doc.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondoubleclick or @onload or @onunload or @onerror]");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.Attributes.Remove("onFocus");
                    node.Attributes.Remove("onBlur");
                    node.Attributes.Remove("onClick");
                    node.Attributes.Remove("onMouseOver");
                    node.Attributes.Remove("onMouseOut");
                    node.Attributes.Remove("onDoubleClick");
                    node.Attributes.Remove("onLoad");
                    node.Attributes.Remove("onUnload");
                    node.Attributes.Remove("onError");
                }
            }

            // remove any style attributes that contain the word expression (IE evaluates this as script)
            nc = doc.DocumentNode.SelectNodes("//*[contains(translate(@style, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'expression')]");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.Attributes.Remove("stYle");
                }
            }

            return doc.DocumentNode.WriteTo();
        }

        public static string RemoveUnwantedTags(string html)
        {

            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var unwantedTagNames = new List<string>
            {
                "span",
                "div"
            };

            var htmlDoc = new HtmlDocument();

            // load html
            htmlDoc.LoadHtml(html);

            var tags = (from tag in htmlDoc.DocumentNode.Descendants()
                        where unwantedTagNames.Contains(tag.Name)
                        select tag).Reverse();


            // find formatting tags
            foreach (var item in tags)
            {
                if (item.PreviousSibling == null)
                {
                    // Prepend children to parent node in reverse order
                    foreach (var node in item.ChildNodes.Reverse())
                    {
                        item.ParentNode.PrependChild(node);
                    }
                }
                else
                {
                    // Insert children after previous sibling
                    foreach (var node in item.ChildNodes)
                    {
                        item.ParentNode.InsertAfter(node, item.PreviousSibling);
                    }
                }

                // remove from tree
                item.Remove();
            }

            // return transformed doc
            return htmlDoc.DocumentNode.WriteContentTo().Trim();
        }

        /// <summary>
        /// No follow all external links in a chunk of Html
        /// </summary>
        /// <param name="html"></param>
        /// <param name="externalClassName"></param>
        /// <returns></returns>
        public static string NofollowExternalLinks(string html, string externalClassName = "external")
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var allLinks = doc.DocumentNode.SelectNodes(@"//a[@href]");
            if (allLinks != null)
            {
                foreach (var link in allLinks)
                {
                    var att = link.Attributes["href"];
                    if (att == null) continue;
                    var href = att.Value;
                    if (href.StartsWith("javascript", StringComparison.InvariantCultureIgnoreCase) || href.StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) continue;

                    var urlNext = new Uri(href, UriKind.RelativeOrAbsolute);

                    // Make it absolute if it's relative
                    if (urlNext.IsAbsoluteUri)
                    {
                        // Absolute so it's external
                        link.Attributes.Append("rel", "nofollow");
                        link.Attributes.Append("class", externalClassName);
                    }
                }
                return doc.DocumentNode.WriteTo();
            }
            return html;
        }

        /// <summary>
        /// Get specific amount of paragraphs from Html
        /// </summary>
        /// <param name="html"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static IList<string> GetAmountOfParagraphsFromHtml(string html, int amount = 1)
        {
            var paragraphs = new List<string>();
            try
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                var nodes = htmlDocument.DocumentNode.SelectNodes("//p");
                if (nodes != null && nodes.Any())
                {
                    foreach (var para in nodes.Take(amount))
                    {
                        if (para != null)
                        {
                            paragraphs.Add(para.InnerHtml);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Do nothing - No need to log
            }
            return paragraphs;
        }

        /// <summary>
        /// Returns the first image found in some HTML
        /// </summary>
        /// <param name="html"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static IList<string> GetAmountOfImagesUrlFromHtml(string html, int amount = 1)
        {
            var images = new List<string>();
            try
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                var nodes = htmlDocument.DocumentNode.SelectNodes("//img");
                if (nodes != null && nodes.Any())
                {
                    foreach (var image in nodes.Take(amount))
                    {
                        if (image != null)
                        {
                            var imageUrl = image.Attributes[@"src"];
                            if (imageUrl != null)
                            {
                                images.Add(imageUrl.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Do nothing
            }

            return images;
        }

        #endregion

        #region Domain / IP

        public static int ReturnCurrentPagingNo()
        {
            var p = HttpContext.Current.Request["p"];
            if (!string.IsNullOrEmpty(p))
            {
                return Convert.ToInt32(p);
            }
            return 1;
        }

        public static string CheckLinkHasHttp(string url)
        {
            return !url.Contains("http://") ? string.Concat("http://", url) : url;
        }

        public static string GetUsersIpAddress()
        {
            var context = HttpContext.Current;
            var serverName = context.Request.ServerVariables["SERVER_NAME"];
            if (serverName.ToLower().Contains("localhost"))
            {
                return serverName;
            }
            var ipList = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            return !string.IsNullOrEmpty(ipList) ? ipList.Split(',')[0] : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        /// <summary>
        /// Gets the full current domain
        /// </summary>
        /// <returns></returns>
        public static string ReturnCurrentDomain()
        {
            var r = HttpContext.Current.Request;
            var builder = new UriBuilder(r.Url.Scheme, r.Url.Host, r.Url.Port);
            return builder.Uri.ToString().TrimEnd('/');
        }
        #endregion

        #region Passwords

        private static readonly Random Rng = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string RandomString(int size)
        {
            var buffer = new char[size];
            for (var i = 0; i < size; i++)
            {
                buffer[i] = Chars[Rng.Next(Chars.Length)];
            }
            return new string(buffer);
        }

        /// <summary>
        /// Create a salt for the password hash (just makes it a bit more complex)
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string CreateSalt(int size)
        {
            // Generate a cryptographic random number.
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        #endregion

        #region Guid

        public static Guid GenerateComb()
        {
            // Fill the destination array with a guid - only the last 6 bytes of a guid are
            // evaluated for sorting on SQL Server, and this algorithm will later overwrite those with
            // 6 bytes that are related to time, and therefore the guids are in generation order as far
            // as SQL Server is concerned.
            // Putting an actual guid in the destination array first helps ensure uniqueness across
            // the remaining bytes. See http://msdn.microsoft.com/en-us/library/ms254976.aspx
            var destinationArray = Guid.NewGuid().ToByteArray();

            // Get clock ticks since 1900 and convert to byte array (we will use last 4 bytes later)
            var time = new DateTime(1900, 1, 1);
            var now = DateTime.UtcNow;
            var ticksSince1900 = new TimeSpan(now.Ticks - time.Ticks);
            var bytesFromClockTicks = BitConverter.GetBytes(ticksSince1900.Days);

            // Get milliseconds from time of day and convert to byte array (we will use last 2 bytes later)
            var timeOfDay = now.TimeOfDay;
            var bytesFromMilliseconds = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333)); // Note that SQL Server is accurate to 3.33 millisecond so we divide by 3.333333,
            // makes us compatible with NEWSEQUENTIALID. Not sure that this is useful...

            // Reverse bytes for storage in SQL server
            Array.Reverse(bytesFromClockTicks);
            Array.Reverse(bytesFromMilliseconds);

            // Replace the last 6 bytes of our Guid. These are the ones SQL server will use when comparing guids
            Array.Copy(bytesFromClockTicks, bytesFromClockTicks.Length - 2, destinationArray, destinationArray.Length - 6, 2);
            Array.Copy(bytesFromMilliseconds, bytesFromMilliseconds.Length - 4, destinationArray, destinationArray.Length - 4, 4);

            return new Guid(destinationArray);
        }

        #endregion

        #region Dates

        /// <summary>
        /// Creates a date format of dd MMMM yyyu
        /// </summary>
        /// <param name="theDate"></param>
        /// <param name="removeYear"> </param>
        /// <returns>A formatted string</returns>
        public static string FormatLongDate(DateTime theDate, bool removeYear = false)
        {
            return removeYear ? theDate.ToString("dd MMMM") : theDate.ToString("dd MMMM yyyy");
        }

        /// <summary>
        /// Converts an object into a date
        /// </summary>
        /// <param name="theDate"></param>
        /// <returns>The date, or a date representing now if object cannot be parsed</returns>
        public static DateTime ParseDate(object theDate)
        {
            DateTime date;
            return DateTime.TryParse(theDate.ToString(), out date) ? date : DateTime.UtcNow;
        }

        public static string FormatDateTime(string date, string format)
        {
            DateTime time;
            if (DateTime.TryParse(date, out time) && !string.IsNullOrEmpty(format))
            {
                format = Regex.Replace(format, @"(?<!\\)((\\\\)*)(S)", "$1" + GetDayNumberSuffix(time));
                return time.ToString(format);
            }
            return string.Empty;
        }

        private static string GetDayNumberSuffix(DateTime date)
        {
            switch (date.Day)
            {
                case 1:
                case 0x15:
                case 0x1f:
                    return @"\s\t";

                case 2:
                case 0x16:
                    return @"\n\d";

                case 3:
                case 0x17:
                    return @"\r\d";
            }
            return @"\t\h";
        }

        public static string GetCurrentMonthName()
        {
            return string.Format("{0:MMMM}", DateTime.UtcNow);
        }

        /// <summary>
        /// Returns the date of the monday of a specific week in a specific year
        /// </summary>
        /// <param name="year"></param>
        /// <param name="weekOfYear"></param>
        /// <returns></returns>
        private static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);
            var firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1,
                                                                              CultureInfo.CurrentCulture.DateTimeFormat.
                                                                                  CalendarWeekRule,
                                                                              CultureInfo.CurrentCulture.DateTimeFormat.
                                                                                  FirstDayOfWeek);
            if (firstWeek <= 1)
            {
                weekOfYear -= 1;
            }
            return firstMonday.AddDays(weekOfYear * 7);
        }

        /// <summary>
        /// Returns the time difference in minutes between two date times
        /// </summary>
        /// <param name="dateone"></param>
        /// <param name="datetwo"></param>
        /// <returns></returns>
        public static double TimeDifferenceInMinutes(DateTime dateone, DateTime datetwo)
        {
            var duration = dateone - datetwo;
            return duration.TotalMinutes;
        }


        /// <summary>
        /// Gets a specific day of the weeks date and the next consectuive Nth days dates, example would be every Fridays date for the current month
        /// </summary>
        /// <param name="dt">The date to start from (usually DateTime.UtcNow)</param>
        /// <param name="weekday">The day of the week to look for</param>
        /// <param name="amounttoshow">How man to return, defaults to next 4</param>
        /// <returns>Returns the date of each of the days</returns>
        public static IEnumerable<DateTime> ReturnNextNthWeekdaysOfMonth(DateTime dt, DayOfWeek weekday, int amounttoshow = 4)
        {
            var days =
                Enumerable.Range(1, DateTime.DaysInMonth(dt.Year, dt.Month)).Select(
                    day => new DateTime(dt.Year, dt.Month, day));

            var weekdays = from day in days
                           where day.DayOfWeek == weekday
                           orderby day.Day ascending
                           select day;

            return weekdays.Take(amounttoshow);
        }

        /// <summary>
        /// Gets a specific day of the weeks date and the next consectuive Nth days dates, example would be every Fridays date for however many you want to show
        /// </summary>
        /// <param name="dt">The date to start from (usually DateTime.UtcNow)</param>
        /// <param name="weekday">The day of the week to look for</param>
        /// <param name="amounttoshow">How man to return, defaults to next 4</param>
        /// <returns>Returns the date of each of the days</returns>
        public static IEnumerable<DateTime> ReturnNextNthWeekdays(DateTime dt, DayOfWeek weekday, int amounttoshow = 4)
        {
            // Find the first future occurance of the day.
            while (dt.DayOfWeek != weekday)
                dt = dt.AddDays(1);

            // Create the entire range of dates required. 
            return Enumerable.Range(0, amounttoshow).Select(i => dt.AddDays(i * 7));
        }

        #endregion

        public static bool UserIsBot()
        {
            if (HttpContext.Current.Request.UserAgent != null)
            {
                var userAgent = HttpContext.Current.Request.UserAgent.ToLower();
                var botKeywords = new List<string>
                {
                    "008",
                    "ABACHOBot",
                    "Accoona-AI-Agent",
                    "AddSugarSpiderBot",
                    "AnyApexBot",
                    "Arachmo",
                    "B-l-i-t-z-B-O-T",
                    "Baiduspider",
                    "BecomeBot",
                    "BeslistBot",
                    "BillyBobBot",
                    "Bimbot",
                    "Bingbot",
                    "BlitzBOT",
                    "boitho.com-dc",
                    "boitho.com-robot",
                    "btbot",
                    "CatchBot",
                    "Cerberian Drtrs",
                    "Charlotte",
                    "ConveraCrawler",
                    "cosmos",
                    "Covario IDS",
                    "DataparkSearch",
                    "DiamondBot",
                    "Discobot",
                    "Dotbot",
                    "EARTHCOM.info",
                    "EmeraldShield.com WebBot",
                    "envolk[ITS]spider",
                    "EsperanzaBot",
                    "Exabot",
                    "FAST Enterprise Crawler",
                    "FAST-WebCrawler",
                    "FDSE robot",
                    "FindLinks",
                    "FurlBot",
                    "FyberSpider",
                    "g2crawler",
                    "Gaisbot",
                    "GalaxyBot",
                    "genieBot",
                    "Gigabot",
                    "Girafabot",
                    "Googlebot",
                    "Googlebot-Image",
                    "GurujiBot",
                    "HappyFunBot",
                    "hl_ftien_spider",
                    "Holmes",
                    "htdig",
                    "iaskspider",
                    "ia_archiver",
                    "iCCrawler",
                    "ichiro",
                    "igdeSpyder",
                    "IRLbot",
                    "IssueCrawler",
                    "Jaxified Bot",
                    "Jyxobot",
                    "KoepaBot",
                    "L.webis",
                    "LapozzBot",
                    "Larbin",
                    "LDSpider",
                    "LexxeBot",
                    "Linguee Bot",
                    "LinkWalker",
                    "lmspider",
                    "lwp-trivial",
                    "mabontland",
                    "magpie-crawler",
                    "Mediapartners-Google",
                    "MJ12bot",
                    "MLBot",
                    "Mnogosearch",
                    "mogimogi",
                    "MojeekBot",
                    "Moreoverbot",
                    "Morning Paper",
                    "msnbot",
                    "MSRBot",
                    "MVAClient",
                    "mxbot",
                    "NetResearchServer",
                    "NetSeer Crawler",
                    "NewsGator",
                    "NG-Search",
                    "nicebot",
                    "noxtrumbot",
                    "Nusearch Spider",
                    "NutchCVS",
                    "Nymesis",
                    "obot",
                    "oegp",
                    "omgilibot",
                    "OmniExplorer_Bot",
                    "OOZBOT",
                    "Orbiter",
                    "PageBitesHyperBot",
                    "Peew",
                    "polybot",
                    "Pompos",
                    "PostPost",
                    "Psbot",
                    "PycURL",
                    "Qseero",
                    "Radian6",
                    "RAMPyBot",
                    "RufusBot",
                    "SandCrawler",
                    "SBIder",
                    "ScoutJet",
                    "Scrubby",
                    "SearchSight",
                    "Seekbot",
                    "semanticdiscovery",
                    "Sensis Web Crawler",
                    "SEOChat::Bot",
                    "SeznamBot",
                    "Shim-Crawler",
                    "ShopWiki",
                    "Shoula robot",
                    "silk",
                    "Sitebot",
                    "Snappy",
                    "sogou spider",
                    "Sosospider",
                    "Speedy Spider",
                    "Sqworm",
                    "StackRambler",
                    "suggybot",
                    "SurveyBot",
                    "SynooBot",
                    "Teoma",
                    "TerrawizBot",
                    "TheSuBot",
                    "Thumbnail.CZ robot",
                    "TinEye",
                    "truwoGPS",
                    "TurnitinBot",
                    "TweetedTimes Bot",
                    "TwengaBot",
                    "updated",
                    "Urlfilebot",
                    "Vagabondo",
                    "VoilaBot",
                    "Vortex",
                    "voyager",
                    "VYU2",
                    "webcollage",
                    "Websquash.com",
                    "wf84",
                    "WoFindeIch Robot",
                    "WomlpeFactory",
                    "Xaldon_WebSpider",
                    "yacy",
                    "Yahoo! Slurp",
                    "Yahoo! Slurp China",
                    "YahooSeeker",
                    "YahooSeeker-Testing",
                    "YandexBot",
                    "YandexImages",
                    "YandexMetrika",
                    "Yasaklibot",
                    "Yeti",
                    "YodaoBot",
                    "yoogliFetchAgent",
                    "YoudaoBot",
                    "Zao",
                    "Zealbot",
                    "zspider",
                    "ZyBorg",
                    "AbiLogicBot",
                    "Link Valet",
                    "Link Validity Check",
                    "LinkExaminer",
                    "LinksManager.com_bot",
                    "Mojoo Robot",
                    "Notifixious",
                    "online link validator",
                    "Ploetz + Zeller",
                    "Reciprocal Link System PRO",
                    "REL Link Checker Lite",
                    "SiteBar",
                    "Vivante Link Checker",
                    "W3C-checklink",
                    "Xenu Link Sleuth",
                    "CSE HTML Validator",
                    "CSSCheck",
                    "Cynthia",
                    "HTMLParser",
                    "P3P Validator",
                    "W3C_CSS_Validator_JFouffa",
                    "W3C_Validator",
                    "WDG_Validator",
                    "Awasu",
                    "Bloglines",
                    "everyfeed-spider",
                    "FeedFetcher-Google",
                    "GreatNews",
                    "Gregarius",
                    "MagpieRSS",
                    "NFReader",
                    "UniversalFeedParser"
                };
                return botKeywords.Any(userAgent.Contains);
            }
            return true;
        }

        public static class EnumUtils
        {
            public static T ReturnEnumValueFromString<T>(string enumValueAsString)
            {
                T returnVal;
                try
                {
                    returnVal = (T)Enum.Parse(typeof(T), enumValueAsString, true);
                }
                catch (ArgumentException)
                {
                    returnVal = default(T);
                }
                return returnVal;
            }
        }

    }

}