using System;
using Dialogue.Logic.Constants;

namespace Dialogue.Logic.Models.Activity
{
    public class BadgeActivity : ActivityBase
    {
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
                           Data = AppConstants.KeyBadgeId + AppConstants.Equality + badge.Id + AppConstants.Separator + AppConstants.KeyUserId + AppConstants.Equality + user.Id,
                           Timestamp = timestamp,
                           Type = ActivityType.BadgeAwarded.ToString()
                       };

        }
    }
}
