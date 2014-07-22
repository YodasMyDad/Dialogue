using System;

namespace Dialogue.Logic.Models.Activity
{
    public class BadgeActivity : ActivityBase
    {
        public const string KeyBadgeId = @"BadgeID";
        public const string KeyUserId = @"UserID";

        public Badge Badge { get; set; }
        public Member User { get; set; }

        /// <summary>
        /// Constructor - useful when constructing a badge activity after reading database
        /// </summary>
        public BadgeActivity(Activity activity, Badge badge, Member user)
        {
            ActivityMapped = activity;
            Badge = badge;
            User = user;
        }

        public static Activity GenerateMappedRecord(Badge badge, Member user, DateTime timestamp)
        {
            return new Activity
                       {
                           // badge=badgeId,user=userId
                           Data = KeyBadgeId + Equality + badge.Id + Separator + KeyUserId + Equality + user.Id,
                           Timestamp = timestamp,
                           Type = ActivityType.BadgeAwarded.ToString()
                       };

        }
    }
}
