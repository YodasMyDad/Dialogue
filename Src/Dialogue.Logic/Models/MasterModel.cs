using System;
using System.Collections.Generic;
using Dialogue.Logic.Constants;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Dialogue.Logic.Models
{
    public abstract class MasterModel : PublishedContentWrapped
    {
        protected MasterModel(IPublishedContent content)
            : base(content)
        {
        }

        private IPublishedContent _rootDialogueNode;
        public IPublishedContent DialogueRoot
        {
            get
            {
                var root = Content.AncestorOrSelf(AppConstants.DocTypeForumRoot);
                if (root == null)
                {
                    throw new InvalidOperationException("Could not find the Dialogue root document for the current rendered page");
                }
                _rootDialogueNode = root;
                return _rootDialogueNode;
            }
        }

        public DialogueSettings Settings
        {
            get { return Dialogue.Settings(DialogueRoot.Id); }
        }

        public string PageTitle { get; set; }
        public string MetaDesc { get; set; }
        public bool HideFromNavigation { get; set; }
        public bool ShowInFooter { get; set; }
        public string ConversionCode { get; set; }
        public IList<int> NodePath { get; set; }

    }
}