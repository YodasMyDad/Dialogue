using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public partial class DialoguePage : MasterModel
    {
        public DialoguePage(IPublishedContent content) : base(content)
        {
        }
    }
}