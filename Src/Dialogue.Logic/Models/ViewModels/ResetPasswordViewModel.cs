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
        [DialogueDisplayName("Email address")]
        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string EmailAddress { get; set; }

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