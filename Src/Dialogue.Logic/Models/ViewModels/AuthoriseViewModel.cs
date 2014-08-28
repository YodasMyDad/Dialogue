using System.Collections.Generic;
using Umbraco.Web;

namespace Dialogue.Logic.Models.ViewModels
{
    public class AuthoriseViewModel : MasterModel
    {
        public AuthoriseViewModel()
            : base(UmbracoContext.Current.PublishedContentRequest.PublishedContent)
        {

        }

        public List<Member> Members { get; set; }
        public List<Post> Posts { get; set; }
        public List<Topic> Topics { get; set; }
    }
}