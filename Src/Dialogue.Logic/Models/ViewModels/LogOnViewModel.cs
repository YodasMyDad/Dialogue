using System.ComponentModel.DataAnnotations;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models.ViewModels
{
    public class LogOnViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        [DialogueDisplayName("Members.Label.Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DialogueDisplayName("Members.Label.Password")]
        public string Password { get; set; }

        [DialogueDisplayName("Members.Label.RememberMe")]
        public bool RememberMe { get; set; }
    }
}