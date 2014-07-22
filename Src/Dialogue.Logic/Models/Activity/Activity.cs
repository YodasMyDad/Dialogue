using System;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Models.Activity
{
    public enum ActivityType
    {
        BadgeAwarded,
        MemberJoined,
        ProfileUpdated,
    }

    public class Activity : Entity
    {
        public Activity()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
