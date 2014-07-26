using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Dialogue.Logic.Controllers
{
    #region Render Controllers
    /// <summary>
    /// Create the controller name to be the same as the doctype you want
    /// in this the doctype is 'Home'
    /// </summary>
    /// Example showing how to use DonutCaching, change web.config for different cache profiles.
    //[DonutOutputCache(CacheProfile = "OneSecond")]
    public partial class DialogueController : BaseController
    {
        private readonly IMemberGroup UsersRole;
        public DialogueController()
        {
            UsersRole = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

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


            // Map the home page into it's model, passing in the render model
            // so we have access to the normal Model.Content etc.. in the view
            // just in case we do need it, although we should be doing all logic in the
            // controllers
            var forumRoot = DialogueMapper.MapRootForum(model.Content);

            //using (_unitOfWorkManager.NewUnitOfWork())
            //{
            //    // loop through the categories and get the permissions
            //    foreach (var category in forumRoot.MainCategories)
            //    {
            //        var permissionSet = _permissions.GetPermissions(category, UsersRole);
            //    }
            //}

            // Return the model to the current template
            return View(PathHelper.GetThemeViewPath("Dialogue"), forumRoot);
        }
    } 
    #endregion

    #region SurfaceControllers
    public class DialogueSurfaceController : BaseSurfaceController
    {
        private readonly TopicService _topicService;
        private readonly PostService _postService;
        private readonly MemberPointsService _pointsService;

        public DialogueSurfaceController()
        {
            _topicService = new TopicService();
            _postService = new PostService();
            _pointsService = new MemberPointsService();
        }

        [ChildActionOnly]
        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public PartialViewResult GetMainStats()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new MainStatsViewModel
                {
                    LatestMembers = MemberService.GetLatestUsers(10).ToDictionary(o => o.UserName,
                                                                                      o => o.NiceUrl),
                    MemberCount = MemberService.MemberCount(),
                    TopicCount = _topicService.TopicCount(),
                    PostCount = _postService.PostCount()
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
                var highEarners = _pointsService.GetCurrentWeeksPoints(20);
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
                    var highEarners = _pointsService.GetCurrentWeeksPoints(20);
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
                    var highEarners = _pointsService.GetThisYearsPoints(20);
                    var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                    return PartialView(PathHelper.GetThemePartialViewPath("GetThisYearsTopEarners"), viewModel);
                }
            }
            return null;
        }

    } 
    #endregion



}