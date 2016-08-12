using Dialogue.Logic.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [DialogueDisplayName("Members.Label.EmailAddress")]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DialogueDisplayName("Members.Label.NewPassword")]
        public string NewPassword { get; set; }

        [Required]
        [DialogueDisplayName("Members.Label.ConfirmNewPassword")]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}