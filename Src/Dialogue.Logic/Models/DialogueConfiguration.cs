namespace Dialogue.Logic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Web.Hosting;
    using System.Xml;
    using Umbraco.Core;

    public class DialogueConfiguration
    {
        private static string ConfigLocation => HostingEnvironment.MapPath("~/App_Plugins/Dialogue/config/dialogue.config");

        #region Singleton
        private static DialogueConfiguration _instance;
        private static readonly object InstanceLock = new object();

        public static DialogueConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DialogueConfiguration();
                        }
                    }
                }

                return _instance;
            }
        }
        #endregion

        #region Membership

        /// <summary>
        /// Default Member Group
        /// </summary>
        public string MemberGroupDefault => GetConfig("MemberGroupDefault");

        /// <summary>
        /// Member Type Alias
        /// </summary>
        public string MemberTypeAlias => GetConfig("MemberTypeAlias");

        /// <summary>
        /// Member Type Name
        /// </summary>
        public string MemberTypeName => GetConfig("MemberTypeName");

        #endregion

        #region Urls

        // Page Names
        public string PageUrlLeaderboard = GetConfig("PageUrlLeaderboard");
        public string PageUrlActivity = GetConfig("PageUrlActivity");
        public string PageUrlFavourites = GetConfig("PageUrlFavourites");
        public string PageUrlPostReport = GetConfig("PageUrlPostReport");
        public string PageUrlEditPost = GetConfig("PageUrlEditPost");
        public string PageUrlMessageInbox = GetConfig("PageUrlMessageInbox");
        public string PageUrlMessageOutbox = GetConfig("PageUrlMessageOutbox");
        public string PageUrlCreatePrivateMessage = GetConfig("PageUrlCreatePrivateMessage");
        public string PageUrlViewPrivateMessage = GetConfig("PageUrlViewPrivateMessage");
        public string PageUrlViewReportMember = GetConfig("PageUrlViewReportMember");
        public string PageUrlEditMember = GetConfig("PageUrlEditMember");
        public string PageUrlChangePassword = GetConfig("PageUrlChangePassword");
        public string PageUrlSearch = GetConfig("PageUrlSearch");
        public string PageUrlTopicsRss = GetConfig("PageUrlTopicsRss");
        public string PageUrlActivityRss = GetConfig("PageUrlActivityRss");
        public string PageUrlCategoryRss = GetConfig("PageUrlCategoryRss");
        public string PageUrlCreateTopic = GetConfig("PageUrlCreateTopic");
        public string PageUrlBadges = GetConfig("PageUrlBadges");
        public string PageUrlEmailConfirmation = GetConfig("PageUrlEmailConfirmation");
        public string PageUrlSpamOverview = GetConfig("PageUrlSpamOverview");
        public string PageUrlAuthorise = GetConfig("PageUrlAuthorise");

        #endregion

        #region DocTypes

        public string DocTypeForumRoot = GetConfig("DocTypeForumRoot");
        public string DocTypeForumCategory = GetConfig("DocTypeForumCategory");
        public string DocTypeLogin = GetConfig("DocTypeLogin");
        public string DocTypeRegister = GetConfig("DocTypeRegister");
        public string DocTypeCreateTopic = GetConfig("DocTypeCreateTopic");
        public string DocTypeEditMember = GetConfig("DocTypeEditMember");
        public string DocTypeSearchMembers = GetConfig("DocTypeSearchMembers");
        public string DocTypeSendPrivateMessage = GetConfig("DocTypeSendPrivateMessage");

        #endregion

        #region Image Sizes

        public int GravatarPostSize = Convert.ToInt32(GetConfig("GravatarPostSize"));
        public int GravatarTopicSize = Convert.ToInt32(GetConfig("GravatarTopicSize"));
        public int GravatarProfileSize = Convert.ToInt32(GetConfig("GravatarProfileSize"));
        public int GravatarLeaderboardSize = Convert.ToInt32(GetConfig("GravatarLeaderboardSize"));

        #endregion

        #region Page Sizes

        public int ActiveTopicsListSize = Convert.ToInt32(GetConfig("ActiveTopicsListSize"));
        public int PagingGroupSize = Convert.ToInt32(GetConfig("PagingGroupSize"));
        public int PrivateMessageListSize = Convert.ToInt32(GetConfig("PrivateMessageListSize"));

        #endregion

        #region Activity Time Checks    

        public int TimeSpanInMinutesToDoCheck = Convert.ToInt32(GetConfig("TimeSpanInMinutesToDoCheck"));
        public int TimeSpanInMinutesToShowMembers = Convert.ToInt32(GetConfig("TimeSpanInMinutesToShowMembers"));

        #endregion

        #region Editor

        public string EditorType = GetConfig("EditorType");

        #endregion

        #region Config Methods

        /// <summary>
        /// Get a config from the default general configuration section
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetConfig(string key)
        {
            var dict = GetForumConfig("/dialogueconfig/general/config");
            if (!string.IsNullOrEmpty(key) && dict.ContainsKey(key))
            {
                return dict[key];
            }
            return string.Empty;
        }

        /// <summary>
        /// Get a config from your own section
        /// </summary>
        /// <param name="key"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        private string GetConfig(string key, string xpath)
        {
            var dict = GetForumConfig(xpath);
            if (!string.IsNullOrEmpty(key) && dict.ContainsKey(key))
            {
                return dict[key];
            }
            return string.Empty;
        }

        /// <summary>
        /// Get a custom set of configs or values from the .config file
        /// </summary>
        /// <param name="xPath"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetForumConfig(string xPath)
        {
            var cacheKey = string.Concat("dialogue-", xPath);

            return (Dictionary<string, string>)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(cacheKey, () =>
            {
                var siteConfig = new Dictionary<string, string>();
                var root = GetSiteConfig();
                var nodes = root?.SelectNodes(xPath);
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        if (node.Attributes != null)
                        {
                            var keyAttr = node.Attributes["key"];
                            var valueAttr = node.Attributes["value"];
                            if (keyAttr != null && valueAttr != null)
                            {
                                siteConfig.Add(keyAttr.InnerText, valueAttr.InnerText);
                            }
                        }
                    }
                }
                return siteConfig;
            });

        }

        /// <summary>
        /// Gets the dialogue.config in an XmlNode
        /// </summary>
        /// <returns></returns>
        private static XmlNode GetSiteConfig()
        {
            const string cacheKey = "dialogueforumsiteconfig";
            return (XmlNode) ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(cacheKey, () =>
            {
                var xDoc = GetXmlDoc(ConfigLocation);
                return xDoc?.DocumentElement;
            });
        }

        /// <summary>
        /// Get any config file as an XMLDocument
        /// </summary>
        /// <param name="pathToConfig"></param>
        /// <returns></returns>
        private static XmlDocument GetXmlDoc(string pathToConfig)
        {
            if (pathToConfig != null)
            {
                var xDoc = new XmlDocument();
                xDoc.Load(pathToConfig);
                return xDoc;
            }
            return null;
        }
        #endregion
    }
}