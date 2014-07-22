using System;

namespace Dialogue.Logic.Models.Activity
{

    public class ProfileUpdatedActivity : ActivityBase
    {
        public const string KeyUserId = @"UserID";

        public Member User { get; set; }
        
        /// <summary>
        /// Constructor - useful when constructing a badge activity after reading database
        /// </summary>
        public ProfileUpdatedActivity(Activity activity, Member user)
        {
            ActivityMapped = activity;
            User = user;
        }

        public static Activity GenerateMappedRecord(Member user, DateTime modified)
        {
            return new Activity
            {
                Data = KeyUserId + Equality + user.Id,
                Timestamp = modified,
                Type = ActivityType.ProfileUpdated.ToString()
            };

        }
    }
}
