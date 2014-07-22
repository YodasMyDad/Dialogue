using System;
using System.ComponentModel.DataAnnotations;

namespace Dialogue.Logic.Models.ViewModels
{
    public class AjaxEditPermissionViewModel
    {
        public bool HasPermission { get; set; }

        public Guid Permission { get; set; }

        public int MemberGroup { get; set; }

        public int Category { get; set; }
    }
}