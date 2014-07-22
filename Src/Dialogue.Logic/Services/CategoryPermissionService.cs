using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class CategoryPermissionService
    {
        public CategoryPermission Add(CategoryPermission categoryPermission)
        {
            return ContextPerRequest.Db.CategoryPermission.Add(categoryPermission);
        }

        public CategoryPermission GetByPermissionRoleCategoryId(Guid permId, int memberGroupId, int catId)
        {
            return ContextPerRequest.Db.CategoryPermission.FirstOrDefault(x => x.CategoryId == catId &&
                                                                           x.Permission.Id == permId &&
                                                                           x.MemberGroupId == memberGroupId);
        }

        public IEnumerable<CategoryPermission> GetAll()
        {
            return ContextPerRequest.Db.CategoryPermission.ToList();
        }

        public List<CategoryPermission> GetCategoryRowList(int memberGroupId, int catId)
        {
            return ContextPerRequest.Db.CategoryPermission
                .Where(x => x.CategoryId == catId &&
                            x.MemberGroupId == memberGroupId)
                            .ToList();
        }

        public IEnumerable<CategoryPermission> GetByCategory(int catgoryId)
        {
            return ContextPerRequest.Db.CategoryPermission
                .Where(x => x.CategoryId == catgoryId)
                .ToList();
        }

        public IEnumerable<CategoryPermission> GetByRole(int memberGroupId)
        {
            return ContextPerRequest.Db.CategoryPermission
                .Where(x => x.MemberGroupId == memberGroupId)
                .ToList();
        }

        public IEnumerable<CategoryPermission> GetByPermission(Guid permId)
        {
            return ContextPerRequest.Db.CategoryPermission
                .Where(x => x.Permission.Id == permId)
                .ToList();
        }

        public CategoryPermission Get(int id)
        {
            return ContextPerRequest.Db.CategoryPermission.FirstOrDefault(cat => cat.CategoryId == id);
        }

        public void Delete(CategoryPermission categoryPermission)
        {
            ContextPerRequest.Db.CategoryPermission.Remove(categoryPermission);
        }


        /// <summary>
        /// Check the category permission for role actually exists
        /// </summary>
        /// <param name="categoryPermission"></param>
        /// <returns></returns>
        public CategoryPermission CheckExists(CategoryPermission categoryPermission)
        {
            if (categoryPermission.Permission != null)
            {

                return GetByPermissionRoleCategoryId(categoryPermission.Permission.Id,
                                                          categoryPermission.MemberGroupId,
                                                          categoryPermission.CategoryId);
            }

            return null;
        }

        /// <summary>
        /// Either updates a CPFR if exists or creates a new one
        /// </summary>
        /// <param name="categoryPermission"></param>
        public void UpdateOrCreateNew(CategoryPermission categoryPermission)
        {
            // Firstly see if this exists already
            var permission = CheckExists(categoryPermission);

            // if it exists then just update it
            if (permission != null)
            {
                permission.IsTicked = categoryPermission.IsTicked;
            }
            else
            {
                Add(categoryPermission);
            }
        }


        /// <summary>
        /// Returns a row with the permission and CP
        /// </summary>
        public Dictionary<Permission, CategoryPermission> GetCategoryRow(int memberGroupId, int catId)
        {
            var catRowList = GetCategoryRowList(memberGroupId, catId);
            return catRowList.ToDictionary(catRow => catRow.Permission);
        }


    }
}