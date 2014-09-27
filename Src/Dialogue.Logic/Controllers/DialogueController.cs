using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Web.Models;

namespace Dialogue.Logic.Controllers
{
    #region Render Controllers

    public partial class DialogueController : BaseRenderController
    {

        /// <summary>
        /// Create an actionresult to target a specific template
        /// in this case the 'Home' template on the 'Home' Doctype
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override ActionResult Index(RenderModel model)
        {

            // This is how we log out
            var logOff = Request.QueryString[AppConstants.LogOut];
            if (!string.IsNullOrEmpty(logOff))
            {
                ServiceFactory.MemberService.LogOff();
                return Redirect(Settings.ForumRootUrl);
            }

            var forumRoot = DialogueMapper.MapRootForum(model.Content);

            // Return the model to the current template
            return View(PathHelper.GetThemeViewPath("Dialogue"), forumRoot);
        }
    } 

    #endregion

    #region SurfaceControllers
    public partial class DialogueSurfaceController : BaseSurfaceController
    {

        [ChildActionOnly]
        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public PartialViewResult GetMainStats()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new MainStatsViewModel
                {
                    LatestMembers = ServiceFactory.MemberService.GetLatestUsers(10).ToDictionary(o => o.UserName,
                                                                                      o => o.Url),
                    MemberCount = ServiceFactory.MemberService.MemberCount(),
                    TopicCount = ServiceFactory.TopicService.TopicCount(),
                    PostCount = ServiceFactory.PostService.PostCount()
                };
                return PartialView(PathHelper.GetThemePartialViewPath("GetMainStats"), viewModel);
            }
        }

        [ChildActionOnly]
        public PartialViewResult GetCurrentActiveMembers()
        {
            var viewModel = new ActiveMembersViewModel
            {
                ActiveMembers = ServiceFactory.MemberService.GetActiveMembers()
            };
            return PartialView(PathHelper.GetThemePartialViewPath("GetCurrentActiveMembers"), viewModel);
        }

        [ChildActionOnly]
        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public PartialViewResult CurrentWeekHighPointUsers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var highEarners = ServiceFactory.MemberPointsService.GetCurrentWeeksPoints(20);
                var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                return PartialView(PathHelper.GetThemePartialViewPath("CurrentWeekHighPointUsers"), viewModel);
            }
        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public PartialViewResult GetThisWeeksTopEarners()
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var highEarners = ServiceFactory.MemberPointsService.GetCurrentWeeksPoints(20);
                    var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                    return PartialView(PathHelper.GetThemePartialViewPath("GetThisWeeksTopEarners"), viewModel);
                }
            }
            return null;
        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public PartialViewResult GetThisYearsTopEarners()
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var highEarners = ServiceFactory.MemberPointsService.GetThisYearsPoints(20);
                    var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                    return PartialView(PathHelper.GetThemePartialViewPath("GetThisYearsTopEarners"), viewModel);
                }
            }
            return null;
        }

    } 
    #endregion

}