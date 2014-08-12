using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models.ViewModels
{
    public class FavouritePostViewModel
    {
        public Guid PostId { get; set; }
    }

    public class ViewFavouritesViewModel : MasterModel
    {
        public ViewFavouritesViewModel(IPublishedContent content) : base(content)
        {
        }

        public List<Post> Posts { get; set; } 
    }
}