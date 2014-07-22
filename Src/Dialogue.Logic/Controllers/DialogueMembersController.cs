using System;
using System.Web.Mvc;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.Controllers
{
    public partial class DialogueMembersSurfaceController : BaseSurfaceController
    {
        private readonly PrivateMessageService _privateMessageService;
        public DialogueMembersSurfaceController()
        {
            _privateMessageService = new PrivateMessageService();
        }

        [Authorize]
        public PartialViewResult SideAdminPanel()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var count = _privateMessageService.NewPrivateMessageCount(CurrentMember.Id);
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
                        MemberService.UpdateLastActiveDate(CurrentMember);
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