using System;

namespace Dialogue.Logic.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ImageAttribute : Attribute
    {
        public string Image { get; set; }

        public ImageAttribute(string name)
        {
            Image = name;
        }
    }
}
