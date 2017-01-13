namespace Dialogue.Logic.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Application;
    using Constants;
    using Mapping;
    using Models;
    using Models.ViewModels;
    using Umbraco.Core.Models;
    using Umbraco.Web.Models;

    public partial class DialogueCategoryController : DialogueBaseController
    {
        private readonly IMemberGroup _usersRole;

        public DialogueCategoryController()
        {           
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
                var permissions = PermissionService.GetPermissions(category, _usersRole, MemberService, CategoryPermissionService);

                if (!permissions[AppConstants.PermissionDenyAccess].IsTicked)
                {

                    var topics = TopicService.GetPagedTopicsByCategory(pageIndex,
                                                                        Settings.TopicsPerPage,
                                                                        int.MaxValue, category.Id);

                    var isSubscribed = UserIsAuthenticated && (CategoryNotificationService.GetByUserAndCategory(CurrentMember, category).Any());

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
                            var permissionSet = PermissionService.GetPermissions(subCategory, _usersRole, MemberService, CategoryPermissionService);
                            subCatViewModel.AllPermissionSets.Add(subCategory, permissionSet);
                        }
                        viewModel.SubCategories = subCatViewModel;
                    }

                    return View(PathHelper.GetThemeViewPath("Category"), viewModel);
                }

                return ErrorToHomePage(Lang("Errors.NoPermission"));
            }
        }

        #region Child Actions

        [ChildActionOnly]
        public PartialViewResult ListCategorySideMenu()
        {
            var catViewModel = new CategoryListViewModel
            {
                AllPermissionSets = new Dictionary<Category, PermissionSet>()
            };

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                foreach (var category in CategoryService.GetAllMainCategories())
                {
                    var permissionSet = PermissionService.GetPermissions(category, _usersRole, MemberService, CategoryPermissionService);
                    catViewModel.AllPermissionSets.Add(category, permissionSet);
                }
            }

            return PartialView(PathHelper.GetThemePartialViewPath("SideCategories"), catViewModel);
        }

        #endregion

    } 

}