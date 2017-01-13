namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System.Text;
    using Application;
    using Models;

    public class ReportService : IRequestCachedService
    {
        public void MemberReport(Report report, EmailService emailService)
        {
            var sb = new StringBuilder();
            var email = new Email();

            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>",
                string.Concat(Dialogue.Settings().ForumRootUrlWithDomain, report.Reporter.Url),
                report.Reporter.UserName,
                AppHelpers.Lang("Report.Reporter"));

            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>",
                string.Concat(Dialogue.Settings().ForumRootUrlWithDomain, report.ReportedMember.Url),
                report.ReportedMember.UserName,
                AppHelpers.Lang("Report.MemberReported"));

            sb.Append($"<p>{AppHelpers.Lang("Report.Reason")}:</p>");
            sb.Append($"<p>{report.Reason}</p>");

            email.EmailFrom = Dialogue.Settings().NotificationReplyEmailAddress;
            email.EmailTo = Dialogue.Settings().AdminEmailAddress;
            email.Subject = AppHelpers.Lang("Report.MemberReport");
            email.NameTo = AppHelpers.Lang("Report.Admin");

            email.Body = emailService.EmailTemplate(email.NameTo, sb.ToString());
            emailService.SendMail(email);
        }

        /// <summary>
        /// Report a post
        /// </summary>
        /// <param name="report"></param>
        /// <param name="emailService"></param>
        public void PostReport(Report report, EmailService emailService)
        {
            var sb = new StringBuilder();
            var email = new Email();

            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>", string.Concat(Dialogue.Settings().ForumRootUrlWithDomain, report.Reporter.Url),
                report.Reporter.UserName,
                AppHelpers.Lang("Report.Reporter"));

            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>", string.Concat(Dialogue.Settings().ForumRootUrlWithDomain,
                report.ReportedPost.Topic.Url), report.ReportedPost.Topic.Name,
                AppHelpers.Lang("Report.PostReported"));

            sb.AppendFormat("<p>{0}:</p>", AppHelpers.Lang("Report.Reason"));
            sb.AppendFormat("<p>{0}</p>", report.Reason);

            email.EmailFrom = Dialogue.Settings().NotificationReplyEmailAddress;
            email.EmailTo = Dialogue.Settings().AdminEmailAddress;
            email.Subject = AppHelpers.Lang("Report.PostReport");
            email.NameTo = AppHelpers.Lang("Report.Admin");

            email.Body = emailService.EmailTemplate(email.NameTo, sb.ToString());

            emailService.SendMail(email);
        }
    }
}