using System;
using Dialogue.Logic.Application;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Models
{
    public partial class CategoryPermission : Entity
    {
        public CategoryPermission()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual Permission Permission { get; set; }
        public IMemberGroup MemberGroup { get; set; }
        public int MemberGroupId { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
        public bool IsTicked { get; set; }
    }

    public partial class CategoryLevelPermission
    {
        public bool HasPointsToAccess { get; set; }
        public bool HasPointsToCreateTopic { get; set; }
        public bool HasPointsToCreatePost { get; set; }
    }
}