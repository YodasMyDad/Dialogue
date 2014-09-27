using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Services
{
    public partial class PermissionService
    {

        private PermissionSet _permissions;


        public IEnumerable<Permission> GetAll()
        {
            return ContextPerRequest.Db.Permission
                .OrderBy(x => x.Name)
                .ToList();
        }

        public Permission Add(Permission permission)
        {
            permission.Name = AppHelpers.SafePlainText(permission.Name);
            return ContextPerRequest.Db.Permission.Add(permission);
        }

        public Permission Get(Guid id)
        {
            return ContextPerRequest.Db.Permission.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Permission item)
        {
            var catPermForRoles = ServiceFactory.CategoryPermissionService.GetByPermission(item.Id);
            foreach (var categoryPermissionForRole in catPermForRoles)
            {
                ServiceFactory.CategoryPermissionService.Delete(categoryPermissionForRole);
            }
            ContextPerRequest.Db.Permission.Remove(item);
        }


        public  Dictionary<int, Dictionary<Permission, bool>> GetFullPermissionTable(List<CategoryPermission> catPermissions)
        {
            var permissionRows = new Dictionary<int, Dictionary<Permission, bool>>();

            foreach (var catPermissionForRole in catPermissions)
            {
                if (!permissionRows.ContainsKey(catPermissionForRole.CategoryId))
                {
                    var permissionList = new Dictionary<Permission, bool>();

                    permissionRows.Add(catPermissionForRole.CategoryId, permissionList);
                }

                if (!permissionRows[catPermissionForRole.CategoryId].ContainsKey(catPermissionForRole.Permission))
                {
                    permissionRows[catPermissionForRole.CategoryId].Add(catPermissionForRole.Permission, catPermissionForRole.IsTicked);
                }
                else
                {
                    permissionRows[catPermissionForRole.CategoryId][catPermissionForRole.Permission] = catPermissionForRole.IsTicked;
                }
            }
            return permissionRows;
        }

        #region PermissionSets

        // var permissionSet = _permissionService.GetPermissions(category, role);
        //if (!permissionSet[AppConstants.PermissionDenyAccess].IsTicked)
        //{
        //    filteredCats.Add(category);
        //}


        /// <summary>
        /// Admin: so no need to check db, admin is all powerful
        /// </summary>
        private PermissionSet GetAdminPermissions(Category category, IMemberGroup memberGroup)
        {
            // Get all permissions
            var permissionList = GetAll();

            // Make a new entry in the results against each permission. All true (this is admin) except "Deny Access" 
            // and "Read Only" which should be false
            var permissionSet = new PermissionSet(permissionList.Select(permission => new CategoryPermission
            {
                Category = category,
                IsTicked = (permission.Name != AppConstants.PermissionDenyAccess && permission.Name != AppConstants.PermissionReadOnly),
                MemberGroup = memberGroup,
                Permission = permission
            }).ToList());


            return permissionSet;

        }

        /// <summary>
        /// Guest = Not logged in, so only need to check the access permission
        /// </summary>
        /// <param name="category"></param>
        /// <param name="memberGroup"></param>
        private PermissionSet GetGuestPermissions(Category category, IMemberGroup memberGroup)
        {
            // Get all the permissions 
            var permissionList = GetAll();

            // Make a CategoryPermissionForRole for each permission that exists,
            // but only set the read-only permission to true for this role / category. All others false
            var permissions = permissionList.Select(permission => new CategoryPermission
            {
                Category = category,
                IsTicked = permission.Name == AppConstants.PermissionReadOnly,
                MemberGroup = memberGroup,
                Permission = permission
            }).ToList();

            
            // Deny Access may have been set (or left null) for guest for the category, so need to read for it
            var denyAccessPermission = ServiceFactory.CategoryPermissionService.GetByRole(memberGroup.Id)
                               .FirstOrDefault(x => x.CategoryId == category.Id &&
                                                    x.Permission.Name == AppConstants.PermissionDenyAccess &&
                                                    x.MemberGroupId == memberGroup.Id);

            // Set the Deny Access value in the results. If it's null for this role/category, record it as false in the results
            var categoryPermissionForRole = permissions.FirstOrDefault(x => x.Permission.Name == AppConstants.PermissionDenyAccess);
            if (categoryPermissionForRole != null)
            {
                categoryPermissionForRole.IsTicked = denyAccessPermission != null && denyAccessPermission.IsTicked;
            }

            var permissionSet = new PermissionSet(permissions);


            return permissionSet;

        }

        /// <summary>
        /// Get permissions for roles other than those specially treated in this class
        /// </summary>
        /// <param name="category"></param>
        /// <param name="memberGroup"></param>
        /// <returns></returns>
        private PermissionSet GetOtherPermissions(Category category, IMemberGroup memberGroup)
        {
            // Get all permissions
            var permissionList = GetAll();

            // Get the known permissions for this role and category
            var categoryRow = ServiceFactory.CategoryPermissionService.GetCategoryRow(memberGroup.Id, category.Id);
            //var categoryRowPermissions = categoryRow.ToDictionary(catRow => catRow.Permission);

            // Load up the results with the permisions for this role / cartegory. A null entry for a permissions results in a new
            // record with a false value
            var permissions = new List<CategoryPermission>();
            foreach (var permission in permissionList)
            {
                permissions.Add(categoryRow.ContainsKey(permission)
                                    ? categoryRow[permission]
                                    : new CategoryPermission { Category = category, MemberGroup = memberGroup, IsTicked = false, Permission = permission });
            }

            var permissionSet = new PermissionSet(permissions);

            return permissionSet;

        }

        /// <summary>
        /// Returns permission set based on category and role
        /// </summary>
        /// <param name="category"></param>
        /// <param name="memberGroup"></param>
        /// <returns></returns>
        public PermissionSet GetPermissions(Category category, IMemberGroup memberGroup)
        {
            if (memberGroup == null)
            {
                // This can only happen if the user has deleted a group, and not reassigned them
                // so in this occasion we just set them to a guest until the admin assigns them a new group
                memberGroup = ServiceFactory.MemberService.GetGroupByName(AppConstants.GuestRoleName);
            }

            // Pass the role in to see select which permissions to apply
            // Going to cache this per request, just to help with performance
            var objectContextKey = string.Concat(HttpContext.Current.GetHashCode().ToString("x"), "-", category.Id, "-", memberGroup.Id);
            if (!HttpContext.Current.Items.Contains(objectContextKey))
            {
                switch (memberGroup.Name)
                {
                    case AppConstants.AdminRoleName:
                        _permissions = GetAdminPermissions(category, memberGroup);
                        break;
                    case AppConstants.GuestRoleName:
                        _permissions = GetGuestPermissions(category, memberGroup);
                        break;
                    default:
                        _permissions = GetOtherPermissions(category, memberGroup);
                        break;
                }

                HttpContext.Current.Items.Add(objectContextKey, _permissions);
            }

            return HttpContext.Current.Items[objectContextKey] as PermissionSet;

        }

        #endregion


    }
}