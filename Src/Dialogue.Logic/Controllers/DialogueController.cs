namespace Dialogue.Logic.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using Constants;
    using Mapping;
    using Models.ViewModels;
    using Umbraco.Web.Models;

    public partial class DialogueController : DialogueBaseController
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
                MemberService.LogOff();
                return Redirect(Settings.ForumRootUrl);
            }

            var forumRoot = DialogueMapper.MapRootForum(model.Content);

            // Return the model to the current template
            return View(PathHelper.GetThemeViewPath("Dialogue"), forumRoot);
        }

        #region Child Actions

        [ChildActionOnly]
        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public PartialViewResult GetMainStats()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new MainStatsViewModel
                {
                    LatestMembers = MemberService.GetLatestUsers(10).ToDictionary(o => o.UserName,
                                                                                      o => o.Url),
                    MemberCount = MemberService.MemberCount(),
                    TopicCount = TopicService.TopicCount(),
                    PostCount = PostService.PostCount()
                };
                return PartialView(PathHelper.GetThemePartialViewPath("GetMainStats"), viewModel);
            }
        }

        [ChildActionOnly]
        public PartialViewResult GetCurrentActiveMembers()
        {
            var viewModel = new ActiveMembersViewModel
            {
                ActiveMembers = MemberService.GetActiveMembers()
            };
            return PartialView(PathHelper.GetThemePartialViewPath("GetCurrentActiveMembers"), viewModel);
        }

        [ChildActionOnly]
        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public PartialViewResult CurrentWeekHighPointUsers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var highEarners = MemberPointsService.GetCurrentWeeksPoints(20);
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
                    var highEarners = MemberPointsService.GetCurrentWeeksPoints(20);
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
                    var highEarners = MemberPointsService.GetThisYearsPoints(20);
                    var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                    return PartialView(PathHelper.GetThemePartialViewPath("GetThisYearsTopEarners"), viewModel);
                }
            }
            return null;
        }

        #endregion

    }



}