using System.ComponentModel.DataAnnotations;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models.ViewModels
{
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
    }
}