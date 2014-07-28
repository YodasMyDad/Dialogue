using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Routes;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Dialogue.Logic.Controllers
{
    #region Render Controllers

    public class DialogueMemberController : BaseController
    {
        private readonly IMemberGroup _membersGroup;

        public DialogueMemberController()
        {
            _membersGroup = (CurrentMember == null ? ServiceFactory.MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        /// <summary>
        /// Used to render the Members profile (virtual node)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="membername">
        /// The slug which we use to look up the member
        /// </param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ActionResult Show(RenderModel model, string membername, int? p = null)
        {
            var memPage = model.Content as DialogueVirtualPage;
            if (memPage == null)
            {
                throw new InvalidOperationException("The RenderModel.Content instance must be of type " + typeof(DialogueVirtualPage));
            }

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var member = ServiceFactory.MemberService.GetUserBySlug(membername, true);
                var loggedonId = UserIsAuthenticated ? CurrentMember.Id : 0;
                var viewModel = new ViewMemberViewModel(model.Content)
                {
                    User = member, 
                    LoggedOnUserId = loggedonId,
                    PageTitle = string.Concat(member.UserName, Lang("Members.ProfileTitle")),
                    PostCount = ServiceFactory.PostService.GetMemberPostCount(member.Id)
                };

                // Get the topic view slug
                return View(PathHelper.GetThemeViewPath("MemberProfile"), viewModel);
            }
        }


    }

    #endregion


    public partial class DialogueMemberSurfaceController : BaseSurfaceController
    {
        private readonly IMemberGroup _membersGroup;

        public DialogueMemberSurfaceController()
        {
            _membersGroup = (CurrentMember == null ? ServiceFactory.MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        [Authorize]
        public PartialViewResult SideAdminPanel()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var count = ServiceFactory.PrivateMessageService.NewPrivateMessageCount(CurrentMember.Id);
                if (count > 0)
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = Lang("Member.HasNewPrivateMessages"),
                        MessageType = GenericMessages.Info
                    });
                }
                return PartialView(PathHelper.GetThemePartialViewPath("SideAdminPanel"),
                            new ViewAdminSidePanelViewModel { CurrentUser = CurrentMember, NewPrivateMessageCount = count });
            }

        }

        [HttpPost]
        public PartialViewResult GetMemberDiscussions(int id)
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    // Get the user discussions, only grab 100 posts
                    var posts = ServiceFactory.PostService.GetByMember(id, 100);

                    // Get the distinct topics
                    var topics = posts.Select(x => x.Topic).Distinct().Take(6).OrderByDescending(x => x.LastPost.DateCreated).ToList();
                    ServiceFactory.TopicService.PopulateCategories(topics);

                    // Get all the categories for this topic collection
                    var categories = topics.Select(x => x.Category).Distinct();

                    // create the view model
                    var viewModel = new ViewMemberDiscussionsViewModel
                    {
                        Topics = topics,
                        AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                        CurrentUser = CurrentMember
                    };

                    // loop through the categories and get the permissions
                    foreach (var category in categories)
                    {
                        var permissionSet = ServiceFactory.PermissionService.GetPermissions(category, _membersGroup);
                        viewModel.AllPermissionSets.Add(category, permissionSet);
                    }

                    return PartialView(PathHelper.GetThemePartialViewPath("GetMemberDiscussions"), viewModel);
                }
            }
            return null;
        }

        public JsonResult LastActiveCheck()
        {
            if (UserIsAuthenticated)
            {
                var rightNow = DateTime.UtcNow;
                var usersDate = CurrentMember.LastActiveDate;

                var span = rightNow.Subtract(usersDate);
                var totalMins = span.TotalMinutes;

                if (totalMins > AppConstants.TimeSpanInMinutesToDoCheck)
                {
                    // Update users last activity date so we can show the latest users online
                    CurrentMember.LastActiveDate = DateTime.UtcNow;

                    // Update
                    try
                    {
                        ServiceFactory.MemberService.UpdateLastActiveDate(CurrentMember);
                    }
                    catch (Exception ex)
                    {

                        LogError(ex);
                    }
                }
            }

            // You can return anything to reset the timer.
            return Json(new { Timer = "reset" }, JsonRequestBehavior.AllowGet);
        }

    }
}