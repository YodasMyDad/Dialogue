using System.Text;
using Dialogue.Logic.Application;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public class ReportService
    {
        public void MemberReport(Report report)
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

            sb.AppendFormat("<p>{0}:</p>", AppHelpers.Lang("Report.Reason"));
            sb.AppendFormat("<p>{0}</p>", report.Reason);

            email.EmailFrom = Dialogue.Settings().NotificationReplyEmailAddress;
            email.EmailTo = Dialogue.Settings().AdminEmailAddress;
            email.Subject = AppHelpers.Lang("Report.MemberReport");
            email.NameTo = AppHelpers.Lang("Report.Admin");

            email.Body = ServiceFactory.EmailService.EmailTemplate(email.NameTo, sb.ToString());
            ServiceFactory.EmailService.SendMail(email);
        }

        /// <summary>
        /// Report a post
        /// </summary>
        /// <param name="report"></param>
        public void PostReport(Report report)
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

            email.Body = ServiceFactory.EmailService.EmailTemplate(email.NameTo, sb.ToString());

            ServiceFactory.EmailService.SendMail(email);
        }
    }
}