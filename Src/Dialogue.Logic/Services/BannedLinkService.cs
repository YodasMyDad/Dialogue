namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System;
    using System.Linq;

    public partial class BannedLinkService : IRequestCachedService
    {
        /// <summary>
        /// If true is means this peice of text/content contains a banned link
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool ContainsBannedLink(string content)
        {
            if (string.IsNullOrEmpty(content)) return false;
  
            foreach (var link in Dialogue.Settings().BannedLinks.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (content.IndexOf(link, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}