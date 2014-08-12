using System;
using System.Web;

namespace Dialogue.Logic.Models.ViewModels
{
    public class AttachFileToPostViewModel
    {
        public HttpPostedFileBase[] Files { get; set; }
        public Guid UploadPostId { get; set; }
    }
}