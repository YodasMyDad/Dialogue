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

        private readonly UnitOfWorkManager _unitOfWorkManager;
        public PropertyEditorsController()
        {
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
                    Category = ServiceFactory.CategoryService.Get(categoryId).ToViewModel(),
                    Permissions = ServiceFactory.PermissionService.GetAll().ToList(),
                    Groups = ServiceFactory.MemberService.GetAll().Where(x => x.Name != AppConstants.AdminRoleName).ToList(),
                    GuestGroupName = AppConstants.GuestRoleName
                };

                var currentPermissions = ServiceFactory.CategoryPermissionService.GetAll()
                                        .Where(x => x.IsTicked && x.CategoryId == categoryId).ToList();
                var currentPermissionList = new List<string>();

                foreach (var catPerm in currentPermissions)
                {
                    currentPermissionList.Add(string.Format("{0}_{1}_{2}", catPerm.Permission.Id, catPerm.CategoryId, catPerm.MemberGroupId));
                }

                permViewModel.CurrentPermissions = currentPermissionList;

                //Do i need this?
                permViewModel.FullPermissionTable = ServiceFactory.PermissionService.GetFullPermissionTable(ServiceFactory.CategoryPermissionService.GetAll().ToList());
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
                        Permission = ServiceFactory.PermissionService.Get(ajaxEditPermissionViewModel.Permission),
                        IsTicked = ajaxEditPermissionViewModel.HasPermission
                    };
                    ServiceFactory.CategoryPermissionService.UpdateOrCreateNew(mappedItem);

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