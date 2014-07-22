using System;
using System.Linq;
using System.Web;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Application.Akismet
{
    public class AkismetHelper
    {
        private readonly DialogueSettings _settings;
        public AkismetHelper()
        {
            _settings = Dialogue.Settings();
        }

        /// <summary>
        /// Check whether a post is spam or not
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public bool IsSpam(Post post)
        {

            // If akisment is is not enable always return false
            if (_settings.EnableAkismetSpamControl == false || string.IsNullOrEmpty(_settings.AkismetKey)) return false;

            // Akisment must be enabled
            var comment = new Comment
            {
                blog = AppHelpers.CheckLinkHasHttp(_settings.ForumRootUrlWithDomain),
                comment_type = "comment",
                comment_author = post.Member.UserName,
                comment_author_email = post.Member.Email,
                comment_content = post.PostContent,
                permalink = String.Empty,
                referrer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"],
                user_agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"],
                user_ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]
            };
            var validator = new Validator(_settings.AkismetKey);
            return validator.IsSpam(comment);
        }

        /// <summary>
        /// Check whether a topic is spam or not
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool IsSpam(Topic topic)
        {
            // If akisment is is not enable always return false
            if (_settings.EnableAkismetSpamControl == false || string.IsNullOrEmpty(_settings.AkismetKey)) return false;

            // Akisment must be enabled
            var firstOrDefault = topic.Posts.FirstOrDefault(x => x.IsTopicStarter);
            if (firstOrDefault != null)
            {
                var comment = new Comment
                {
                    blog = AppHelpers.CheckLinkHasHttp(_settings.ForumRootUrlWithDomain),
                    comment_type = "comment",
                    comment_author = topic.Member.UserName,
                    comment_author_email = topic.Member.Email,
                    comment_content = firstOrDefault.PostContent,
                    permalink = String.Empty,
                    referrer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"],
                    user_agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"],
                    user_ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]
                };
                var validator = new Validator(_settings.AkismetKey);
                return validator.IsSpam(comment);
            }
            return true;
        }
    }
}