using System;
using Dialogue.Logic.Constants;

namespace Dialogue.Logic.Models.Activity
{

    public class ProfileUpdatedActivity : ActivityBase
    {
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
                Data = AppConstants.KeyUserId + AppConstants.Equality + user.Id,
                Timestamp = modified,
                Type = ActivityType.ProfileUpdated.ToString()
            };

        }
    }
}
