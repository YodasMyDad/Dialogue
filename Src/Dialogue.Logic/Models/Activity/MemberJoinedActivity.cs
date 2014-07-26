using Dialogue.Logic.Constants;

namespace Dialogue.Logic.Models.Activity
{
    // Seal this class to avoid "virtual member call in constructor" problem

    public sealed class MemberJoinedActivity : ActivityBase
    {
        public Member User { get; set; }
        
        /// <summary>
        /// Constructor - useful when constructing a badge activity after reading database
        /// </summary>
        public MemberJoinedActivity(Activity activity, Member user)
        {
            ActivityMapped = activity;
            User = user;
        }

        public static Activity GenerateMappedRecord(Member user)
        {
            return new Activity
            {
                Data = AppConstants.KeyUserId + AppConstants.Equality + user.Id,
                Timestamp = user.DateCreated,
                Type = ActivityType.MemberJoined.ToString()
            };

        }
    }
}
