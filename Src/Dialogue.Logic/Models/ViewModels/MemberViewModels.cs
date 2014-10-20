using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ApproveMemberViewModel
    {
        public int Id { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [DialogueDisplayName("Members.Label.Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [DialogueDisplayName("Members.Label.EmailAddress")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DialogueDisplayName("Members.Label.Password ")]
        public string Password { get; set; }

        public string SpamAnswer { get; set; }
        public int? ForumId { get; set; }
        public string ReturnUrl { get; set; }
        public string SocialProfileImageUrl { get; set; }
        public string UserAccessToken { get; set; }
        public LoginType LoginType { get; set; }
    }

    public class PageReportMemberViewModel : MasterModel
    {
        public PageReportMemberViewModel(IPublishedContent content) : base(content)
        {
        }

        public int MemberId { get; set; }
        public string Username { get; set; }
        public string Reason { get; set; }
    }

    public class ReportMemberViewModel
    {
        public int MemberId { get; set; }
        public string Username { get; set; }
        public string Reason { get; set; }
    }

    public class PageMemberEditViewModel : MasterModel
    {
        public PageMemberEditViewModel(IPublishedContent content) : base(content)
        {
        }

        public MemberEditViewModel MemberEditViewModel { get; set; }
    }

    public class PostMemberEditViewModel
    {
        public MemberEditViewModel MemberEditViewModel { get; set; }
    }

    public class MemberEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [DialogueDisplayName("Members.Label.Username")]
        [StringLength(150, MinimumLength = 2)]
        public string UserName { get; set; }

        [DialogueDisplayName("Members.Label.EmailAddress")]
        [DataType(DataType.EmailAddress)]
        [Required]
        public string Email { get; set; }

        [DialogueDisplayName("Members.Label.Signature")]
        [StringLength(1000)]
        [AllowHtml]
        public string Signature { get; set; }

        [DialogueDisplayName("Members.Label.Website")]
        [Url]
        [StringLength(100)]
        public string Website { get; set; }

        [DialogueDisplayName("Members.Label.UploadNewAvatar")]
        public HttpPostedFileBase[] Files { get; set; }

        public string Avatar { get; set; }

        [DialogueDisplayName("Members.Label.Twitter")]
        public string Twitter { get; set; }

        // Admin Stuff

        [DialogueDisplayName("Members.Label.DisableEmailNotifications")]
        public bool DisableEmailNotifications { get; set; }

        [DialogueDisplayName("Members.Label.DisablePosting")]
        public bool DisablePosting { get; set; }

        [DialogueDisplayName("Members.Label.DisablePrivateMessages")]
        public bool DisablePrivateMessages { get; set; }

        [DialogueDisplayName("Members.Label.DisableFileUploads")]
        public bool DisableFileUploads { get; set; }

        [DialogueDisplayName("Members.Label.CanEditOtherMembers")]
        public bool CanEditOtherMembers { get; set; }

        [DialogueDisplayName("Members.Label.Comment")]
        public string Comments { get; set; }
    }

    public class PageChangePasswordViewModel : MasterModel
    {
        public PageChangePasswordViewModel(IPublishedContent content) : base(content)
        {
        }

        public ChangePasswordViewModel ChangePasswordViewModel { get; set; }
    }

    public class PostChangePasswordViewModel
    {
        public ChangePasswordViewModel ChangePasswordViewModel { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [DialogueDisplayName("Members.Label.CurrentPassword")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [DialogueDisplayName("Members.Label.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [DialogueDisplayName("Members.Label.ConfirmNewPassword")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

}