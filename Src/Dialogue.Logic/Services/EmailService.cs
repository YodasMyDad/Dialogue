using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Dialogue.Logic.Application;
using Dialogue.Logic.Models;
using umbraco;

namespace Dialogue.Logic.Services
{
    public partial class EmailService
    {
        /// <summary>
        /// Returns the HTML email template with values replaced
        /// </summary>
        /// <param name="to"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public string EmailTemplate(string to, string content)
        {
            using (var sr = File.OpenText(HttpContext.Current.Server.MapPath(@"~/App_Plugins/Dialogue/EmailTemplates/EmailNotification.htm")))
            {
                var sb = sr.ReadToEnd();
                sr.Close();
                sb = sb.Replace("#CONTENT#", content);
                sb = sb.Replace("#SITENAME#", Dialogue.Settings().ForumName);
                sb = sb.Replace("#SITEURL#", AppHelpers.ReturnCurrentDomain());
                if (!string.IsNullOrEmpty(to))
                {
                    to = string.Format("<p>{0},</p>", to);
                    sb = sb.Replace("#TO#", to);
                }

                return sb;
            }
        }

        /// <summary>
        /// Send a single email
        /// </summary>
        /// <param name="email"></param>
        public void SendMail(Email email)
        {
            SendMail(new List<Email> { email });
        }

        /// <summary>
        /// Send multiple emails
        /// </summary>
        public void SendMail(List<Email> email)
        {
            try
            {
                if (email != null && email.Count > 0)
                {
                    foreach (var emailMessage in email)
                    {
                        library.SendMail(emailMessage.EmailFrom, emailMessage.EmailTo, emailMessage.Subject, emailMessage.Body, true);
                    }
                }
            }
            catch (Exception ex)
            {
                AppHelpers.LogError("Error sending email", ex);
            }
        }
    }
}