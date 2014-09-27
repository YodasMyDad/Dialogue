using System;
using System.Web.Mvc;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.Controllers
{
    #region Surface Controllers
    public partial class DialogueFavouriteSurfaceController : BaseSurfaceController
    {
        [HttpPost]
        [Authorize]
        public ActionResult FavouritePost(FavouritePostViewModel viewModel)
        {
            if (Request.IsAjaxRequest() && CurrentMember != null)
            {
                using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var post = ServiceFactory.PostService.Get(viewModel.PostId);
                        string returnValue;

                        // See if this is a user adding or removing the favourite
                        var existingFavourite = ServiceFactory.FavouriteService.GetByMemberAndPost(CurrentMember.Id, post.Id);
                        if (existingFavourite != null)
                        {
                            ServiceFactory.FavouriteService.Delete(existingFavourite);
                            returnValue = Lang("Post.Favourite");
                        }
                        else
                        {
                            var favourite = new Favourite
                            {
                                DateCreated = DateTime.UtcNow,
                                MemberId = CurrentMember.Id,
                                PostId = post.Id,
                                TopicId = post.Topic.Id
                            };
                            ServiceFactory.FavouriteService.Add(favourite);
                            returnValue = Lang("Post.Favourited");
                        }
                        unitOfwork.Commit();
                        return Content(returnValue);
                    }
                    catch (Exception ex)
                    {
                        unitOfwork.Rollback();
                        LogError(ex);
                        throw new Exception(Lang("Errors.Generic"));
                    }
                }
            }
            return Content("error");
        }


    } 
    #endregion
}