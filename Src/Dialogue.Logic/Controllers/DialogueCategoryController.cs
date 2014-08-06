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

    #region Render Controllers
    public partial class DialogueCategoryController : BaseRenderController
    {
        private readonly IMemberGroup _usersRole;

        public DialogueCategoryController()
        {           
            _usersRole = (CurrentMember == null ? ServiceFactory.MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
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
                var permissions = ServiceFactory.PermissionService.GetPermissions(category, _usersRole);

                if (!permissions[AppConstants.PermissionDenyAccess].IsTicked)
                {

                    var topics = ServiceFactory.TopicService.GetPagedTopicsByCategory(pageIndex,
                                                                        Settings.TopicsPerPage,
                                                                        int.MaxValue, category.Id);

                    var isSubscribed = UserIsAuthenticated && (ServiceFactory.CategoryNotificationService.GetByUserAndCategory(CurrentMember, category).Any());

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
                            var permissionSet = ServiceFactory.PermissionService.GetPermissions(subCategory, _usersRole);
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
    #endregion

    #region Surface Controllers
    public partial class DialogueCategorySurfaceController : BaseSurfaceController
    {
        private readonly IMemberGroup _usersRole;

        public DialogueCategorySurfaceController()
        {
            _usersRole = (CurrentMember == null ? ServiceFactory.MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
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
                foreach (var category in ServiceFactory.CategoryService.GetAllMainCategories())
                {
                    var permissionSet = ServiceFactory.PermissionService.GetPermissions(category, _usersRole);
                    catViewModel.AllPermissionSets.Add(category, permissionSet);
                }
            }

            return PartialView(PathHelper.GetThemePartialViewPath("SideCategories"), catViewModel);
        }
    }

    #endregion
}