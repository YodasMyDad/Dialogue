using System;

namespace Dialogue.Logic.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public DisplayNameAttribute(string desc)
        {
            DisplayName = desc;
        }
    }
}
