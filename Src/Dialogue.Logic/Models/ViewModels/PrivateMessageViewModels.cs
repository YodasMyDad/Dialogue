using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    #region PageModels

    public class PageListPrivateMessageViewModel : MasterModel
    {
        public PageListPrivateMessageViewModel(IPublishedContent content) : base(content)
        {
        }

        public ListPrivateMessageViewModel ListPrivateMessageViewModel { get; set; }
    }

    public class PageCreatePrivateMessageViewModel : MasterModel
    {
        public PageCreatePrivateMessageViewModel(IPublishedContent content) : base(content)
        {
        }

        public CreatePrivateMessageViewModel CreatePrivateMessageViewModel { get; set; }
    }

    #endregion

    #region ViewModels
    public class ListPrivateMessageViewModel
    {
        public IList<PrivateMessage> Messages { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }

    public class CreatePrivateMessageViewModel
    {
        [DialogueDisplayName("PM.RecipientUsername")]
        [StringLength(150)]
        [Required]
        public string UserToUsername { get; set; }

        [DialogueDisplayName("PM.MessageSubject")]
        [Required]
        public string Subject { get; set; }

        [UIHint(AppConstants.EditorType), AllowHtml]
        public string Message { get; set; }

        public string PreviousMessage { get; set; }

    }

    public class ViewPrivateMessageViewModel : MasterModel
    {
        public ViewPrivateMessageViewModel(IPublishedContent content) : base(content)
        {
        }

        public PrivateMessage Message { get; set; }
    }

    public class DeletePrivateMessageViewModel
    {
        public Guid Id { get; set; }
    }
    
    #endregion
}