using System;

namespace Dialogue.Logic.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public DescriptionAttribute(string desc)
        {
            Description = desc;
        }
    }
}
