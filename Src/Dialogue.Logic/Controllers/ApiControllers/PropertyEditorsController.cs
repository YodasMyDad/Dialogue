using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Core.IO;
using umbraco.presentation.webservices;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Dialogue.Logic.Mapping;

namespace Dialogue.Logic.Controllers.ApiControllers
{
    [PluginController("Dialogue")]
    public class PropertyEditorsController : UmbracoAuthorizedApiController
    {
        // To debug use the non Json one
        //UmbracoAuthorizedApiController
        //UmbracoAuthorizedJsonController

        private readonly PermissionService _permissions;
        private readonly CategoryPermissionService _categoryPermissionService;
        private readonly CategoryService _categoryService;
        private readonly MemberService _memberService;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        public PropertyEditorsController()
        {
            _permissions = new PermissionService();
            _categoryService = new CategoryService();
            _memberService = new MemberService();
            _categoryPermissionService = new CategoryPermissionService();
            _unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
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
                    Permissions = _permissions.GetAll().ToList(),
                    Groups = _memberService.GetAll().Where(x => x.Name != AppConstants.AdminRoleName).ToList(),
                    GuestGroupName = AppConstants.GuestRoleName
                };

                var currentPermissions = _categoryPermissionService.GetAll()
                                        .Where(x => x.IsTicked && x.CategoryId == categoryId).ToList();
                var currentPermissionList = new List<string>();

                foreach (var catPerm in currentPermissions)
                {
                    currentPermissionList.Add(string.Format("{0}_{1}_{2}", catPerm.Permission.Id, catPerm.CategoryId, catPerm.MemberGroupId));
                }

                permViewModel.CurrentPermissions = currentPermissionList;

                //Do i need this?
                permViewModel.FullPermissionTable = _permissions.GetFullPermissionTable(_categoryPermissionService.GetAll().ToList());
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
                        Permission = _permissions.Get(ajaxEditPermissionViewModel.Permission),
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