namespace Dialogue.Logic.Controllers.ApiControllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Http;
    using Application;
    using Constants;
    using Data.Context;
    using Data.UnitOfWork;
    using Models;
    using Models.ViewModels;
    using Services;
    using Umbraco.Core.IO;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.WebApi;
    using Mapping;

    [PluginController("Dialogue")]
    public class PropertyEditorsController : UmbracoAuthorizedApiController
    {
        // To debug use the non Json one
        //UmbracoAuthorizedApiController
        //UmbracoAuthorizedJsonController

        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly CategoryService _categoryService;
        private readonly PermissionService _permissionService;
        private readonly MemberService _memberService;
        private readonly CategoryPermissionService _categoryPermissionService;

        public PropertyEditorsController()
        {
            _unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
            _categoryService = ServiceResolver.Current.Instance<CategoryService>();
            _permissionService = ServiceResolver.Current.Instance<PermissionService>();
            _memberService = ServiceResolver.Current.Instance<MemberService>();
            _categoryPermissionService = ServiceResolver.Current.Instance<CategoryPermissionService>();
        }

        #region Themes
        public IEnumerable<string> GetThemes()
        {
            var dir = IOHelper.MapPath("~/App_Plugins/Dialogue/Themes");
            return Directory.GetDirectories(dir).Select(x => new DirectoryInfo(x).Name);
        }
        #endregion

        #region Permissions

        [HttpGet]
        public EditPermissionsViewModel EditPermissions(int categoryId)
        {
            using (_unitOfWorkManager.NewUnitOfWork())
            {
                var permViewModel = new EditPermissionsViewModel
                {
                    Category = _categoryService.Get(categoryId).ToViewModel(),
                    Permissions = _permissionService.GetAll().ToList(),
                    Groups = _memberService.GetAll().Where(x => x.Name != AppConstants.AdminRoleName).ToList(),
                    GuestGroupName = AppConstants.GuestRoleName
                };

                var currentPermissions = _categoryPermissionService.GetAll()
                                        .Where(x => x.IsTicked && x.CategoryId == categoryId).ToList();
                var currentPermissionList = new List<string>();

                foreach (var catPerm in currentPermissions)
                {
                    currentPermissionList.Add($"{catPerm.Permission.Id}_{catPerm.CategoryId}_{catPerm.MemberGroupId}");
                }

                permViewModel.CurrentPermissions = currentPermissionList;

                //Do i need this?
                permViewModel.FullPermissionTable = _permissionService.GetFullPermissionTable(_categoryPermissionService.GetAll().ToList());
                return permViewModel;
            }
        }

        [HttpPost]
        public void UpdatePermission(AjaxEditPermissionViewModel ajaxEditPermissionViewModel)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var mappedItem = new CategoryPermission
                    {
                        CategoryId = ajaxEditPermissionViewModel.Category,
                        MemberGroupId = ajaxEditPermissionViewModel.MemberGroup,
                        Permission = _permissionService.Get(ajaxEditPermissionViewModel.Permission),
                        IsTicked = ajaxEditPermissionViewModel.HasPermission
                    };
                    _categoryPermissionService.UpdateOrCreateNew(mappedItem);

                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    AppHelpers.LogError("Error updating permissions on category", ex);
                    throw;
                }
            }
        }

        #endregion
    }
}