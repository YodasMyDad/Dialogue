namespace Dialogue.Logic.Models
{
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

        #endregion

        #region DocTypes

        #endregion

        #region Image Sizes

        #endregion

        #region Page Sizes

        #endregion

        #region Activity Time Checks    

        #endregion

        #region Editors

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