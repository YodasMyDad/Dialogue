using System.ComponentModel.DataAnnotations;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [DialogueDisplayName("Members.Label.EnterEmail")]
        public string EmailAddress { get; set; }
    }
}