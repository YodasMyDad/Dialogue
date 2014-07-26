using System;

namespace Dialogue.Logic.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IdAttribute : Attribute
    {
        public Guid Id { get; set; }

        public IdAttribute(string guid)
        {
            Id = new Guid(guid);
        }
    }
}
