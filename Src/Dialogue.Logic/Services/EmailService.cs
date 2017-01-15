namespace Dialogue.Logic.Services
{
    using System.Text;
    using Constants;
    using Umbraco.Core.Models;
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Application;
    using Interfaces;
    using Models;
    using umbraco;
    using File = System.IO.File;

    public partial class EmailService : IRequestCachedService
    {
        /// <summary>
        /// Send the email confirmations
        /// </summary>
        /// <param name="userToSave"></param>
        /// <param name="settings"></param>
        public void SendEmailConfirmationEmail(IMember userToSave, DialogueSettings settings)
        {
            // Ensure we have an umbraco context
            ContextHelper.EnsureUmbracoContext();

            // Make sure correct culture on Ajax Call
            ContextHelper.EnsureCorrectCulture();

            var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
            var memberEmailAuthorisationNeeded = settings.NewMembersMustConfirmAccountsViaEmail;
            if (manuallyAuthoriseMembers == false && memberEmailAuthorisationNeeded)
            {
                if (!string.IsNullOrEmpty(userToSave.Email))
                {
                    // SEND AUTHORISATION EMAIL
                    var sb = new StringBuilder();
                    var confirmationLink = string.Concat(AppHelpers.ReturnCurrentDomain(), Urls.GenerateUrl(Urls.UrlType.EmailConfirmation), "?id=", userToSave.Id);
                    sb.AppendFormat("<p>{0}</p>", string.Format(AppHelpers.Lang("Members.MemberEmailAuthorisation.EmailBody"),
                                                settings.ForumName,
                                                string.Format("<p><a href=\"{0}\">{0}</a></p>", confirmationLink)));
                    var email = new Email
                    {
                        EmailFrom = settings.NotificationReplyEmailAddress,
                        EmailTo = userToSave.Email,
                        NameTo = userToSave.Username,
                        Subject = AppHelpers.Lang("Members.MemberEmailAuthorisation.Subject")
                    };

                    email.Body = EmailTemplate(email.NameTo, sb.ToString());
                    SendMail(email);

                    // ADD COOKIE
                    // We add a cookie for 7 days, which will display the resend email confirmation button
                    // This cookie is removed when they click the confirmation link and they are logged in
                    var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                    {
                        Value = $"{userToSave.Id}#{userToSave.Username}",
                        Expires = DateTime.Now.AddDays(7)
                    };
                    // Add the cookie.
                    HttpContext.Current.Response.Cookies.Add(myCookie);
                }
            }
        }

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
                    to = $"<p>{to},</p>";
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