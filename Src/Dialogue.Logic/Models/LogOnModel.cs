using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public class LogOnModel : MasterModel
    {
        public LogOnModel(IPublishedContent content) : base(content)
        {
        }

        public bool ShowForgotPassword { get; set; }
    }
}