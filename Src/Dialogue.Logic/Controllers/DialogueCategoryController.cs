using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Dialogue.Logic.Controllers
{

    //[DonutOutputCache(CacheProfile = "OneSecond")]
    public partial class DialogueCategoryController : BaseController
    {
        private readonly CategoryService _categoryService;
        private readonly TopicService _topicService;
        private readonly CategoryNotificationService _categoryNotificationService;
        private readonly IMemberGroup _usersRole;

        public DialogueCategoryController()
        {
            _categoryService = new CategoryService();
            _topicService = new TopicService();
            _categoryNotificationService = new CategoryNotificationService();
            _usersRole = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        public override ActionResult Index(RenderModel model)
        {

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Get the category
                var category = CategoryMapper.MapCategory(model.Content, true);

                // Set the page index
                var pageIndex = AppHelpers.ReturnCurrentPagingNo();

                // check the user has permission to this category
                var permissions = PermissionService.GetPermissions(category, _usersRole);

                if (!permissions[AppConstants.PermissionDenyAccess].IsTicked)
                {

                    var topics = _topicService.GetPagedTopicsByCategory(pageIndex,
                                                                        Settings.TopicsPerPage,
                                                                        int.MaxValue, category.Id);

                    var isSubscribed = UserIsAuthenticated && (_categoryNotificationService.GetByUserAndCategory(CurrentMember, category).Any());

                    // Create the main view model for the category
                    var viewModel = new ViewCategoryViewModel(model.Content)
                    {
                        Permissions = permissions,
                        Topics = topics,
                        Category = category,
                        PageIndex = pageIndex,
                        TotalCount = topics.TotalCount,
                        User = CurrentMember,
                        IsSubscribed = isSubscribed
                    };

                    // If there are subcategories then add then with their permissions
                    if (category.SubCategories.Any())
                    {
                        var subCatViewModel = new CategoryListViewModel
                        {
                            AllPermissionSets = new Dictionary<Category, PermissionSet>()
                        };
                        foreach (var subCategory in category.SubCategories)
                        {
                            var permissionSet = PermissionService.GetPermissions(subCategory, _usersRole);
                            subCatViewModel.AllPermissionSets.Add(subCategory, permissionSet);
                        }
                        viewModel.SubCategories = subCatViewModel;
                    }

                    return View(PathHelper.GetThemeViewPath("Category"), viewModel);
                }

                return ErrorToHomePage(Lang("Errors.NoPermission"));
            }           
        }
    }

    public partial class DialogueCategorySurfaceController : BaseSurfaceController
    {
        private readonly IMemberGroup _usersRole;
        private readonly CategoryService _categoryService;

        public DialogueCategorySurfaceController()
        {
            _usersRole = (CurrentMember == null ? MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
            _categoryService = new CategoryService();
        }


        [ChildActionOnly]
        public PartialViewResult ListCategorySideMenu()
        {
            var catViewModel = new CategoryListViewModel
            {
                AllPermissionSets = new Dictionary<Category, PermissionSet>()
            };

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                foreach (var category in _categoryService.GetAllMainCategories())
                {
                    var permissionSet = PermissionService.GetPermissions(category, _usersRole);
                    catViewModel.AllPermissionSets.Add(category, permissionSet);
                }
            }

            return PartialView(PathHelper.GetThemePartialViewPath("SideCategories"), catViewModel);
        }
    }

}