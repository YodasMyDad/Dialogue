namespace Dialogue.Logic.Controllers
{
    using System;
    using System.Web.Mvc;
    using Application;
    using Models;
    using Models.ViewModels;

    public partial class DialogueFavouriteController : DialogueBaseController
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
                        var post = PostService.Get(viewModel.PostId);
                        string returnValue;

                        // See if this is a user adding or removing the favourite
                        var existingFavourite = FavouriteService.GetByMemberAndPost(CurrentMember.Id, post.Id);
                        if (existingFavourite != null)
                        {
                            FavouriteService.Delete(existingFavourite);
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
                            FavouriteService.Add(favourite);
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

}