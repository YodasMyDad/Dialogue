using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.Activity;

namespace Dialogue.Logic.Services
{
    public partial class ActivityService
    {       
        #region There are the methods that power the activity service

        /// <summary>
        /// New badge has been awarded
        /// </summary>
        /// <param name="badge"></param>
        /// <param name="user"> </param>
        /// <param name="timestamp"> </param>
        public void BadgeAwarded(Badge badge, Member user, DateTime timestamp)
        {
            var badgeActivity = BadgeActivity.GenerateMappedRecord(badge, user, timestamp);
            Add(badgeActivity);
        }

        /// <summary>
        /// New member has joined
        /// </summary>
        /// <param name="user"></param>
        public void MemberJoined(Member user)
        {
            var memberJoinedActivity = MemberJoinedActivity.GenerateMappedRecord(user);
            Add(memberJoinedActivity);
        }

        /// <summary>
        /// Profile has been updated
        /// </summary>
        /// <param name="user"></param>
        public void ProfileUpdated(Member user)
        {
            var profileUpdatedActivity = ProfileUpdatedActivity.GenerateMappedRecord(user, DateTime.UtcNow);
            Add(profileUpdatedActivity);
        }

        #endregion


        /// <summary>
        /// Get activities
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Activity> GetAll()
        {
            return ContextPerRequest.Db.Activity;
        }

        /// <summary>
        /// Gets all activities by search data field for a int
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Activity> GetDataByUserId(int id)
        {
            var key = string.Concat(AppConstants.KeyUserId, AppConstants.Equality, id);
            return ContextPerRequest.Db.Activity.Where(x => x.Data.Contains(key));
        }

        /// <summary>
        /// Add a new activity (expected id already assigned)
        /// </summary>
        /// <param name="newActivity"></param>
        public Activity Add(Activity newActivity)
        {
            return ContextPerRequest.Db.Activity.Add(newActivity);
        }


        public Activity Get(Guid id)
        {
            return ContextPerRequest.Db.Activity.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Activity item)
        {
            ContextPerRequest.Db.Activity.Remove(item);
        }



        /// <summary>
        /// Make a badge activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private BadgeActivity GenerateBadgeActivity(Activity activity)
        {
            // Get the corresponding badge
            var dataPairs = ActivityBase.UnpackData(activity);

            if (!dataPairs.ContainsKey(AppConstants.KeyBadgeId))
            {
                // Log the problem then skip
                AppHelpers.LogError(string.Format("A badge activity record with id '{0}' has no badge id in its data.", activity.Id));
                return null;
            }

            var badgeId = dataPairs[AppConstants.KeyBadgeId];
            var badge = ServiceFactory.BadgeService.Get(new Guid(badgeId));

            if (badge == null)
            {
                // Log the problem then skip
                AppHelpers.LogError(string.Format("A badge activity record with id '{0}' has a badge id '{1}' that is not found in the badge table.",
                    activity.Id, badgeId));
                return null;
            }

            var userId = dataPairs[AppConstants.KeyUserId];
            var user = ServiceFactory.MemberService.Get(Convert.ToInt32(userId));

            if (user == null)
            {
                // Log the problem then skip
                AppHelpers.LogError(string.Format("A badge activity record with id '{0}' has a user id '{1}' that is not found in the user table.",
                    activity.Id, userId));
                return null;
            }

            return new BadgeActivity(activity, badge, user);
        }

        /// <summary>
        /// Make a profile updated joined activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private ProfileUpdatedActivity GenerateProfileUpdatedActivity(Activity activity)
        {
            var dataPairs = ActivityBase.UnpackData(activity);

            if (!dataPairs.ContainsKey(AppConstants.KeyUserId))
            {
                // Log the problem then skip
                AppHelpers.LogError(string.Format("A profile updated activity record with id '{0}' has no user id in its data.", activity.Id));
                return null;
            }

            var userId = dataPairs[AppConstants.KeyUserId];
            var user = ServiceFactory.MemberService.Get(Convert.ToInt32(userId));

            if (user == null)
            {
                // Log the problem then skip
                AppHelpers.LogError(string.Format("A profile updated activity record with id '{0}' has a user id '{1}' that is not found in the user table.",
                    activity.Id, userId));
                return null;
            }

            return new ProfileUpdatedActivity(activity, user);
        }

        /// <summary>
        /// Make a member joined activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private MemberJoinedActivity GenerateMemberJoinedActivity(Activity activity)
        {
            var dataPairs = ActivityBase.UnpackData(activity);

            if (!dataPairs.ContainsKey(AppConstants.KeyUserId))
            {
                // Log the problem then skip
                AppHelpers.LogError(string.Format("A member joined activity record with id '{0}' has no user id in its data.", activity.Id));
                return null;
            }

            var userId = dataPairs[AppConstants.KeyUserId];
            var user = ServiceFactory.MemberService.Get(Convert.ToInt32(userId));

            if (user == null)
            {
                // Log the problem then skip
                AppHelpers.LogError(string.Format("A member joined activity record with id '{0}' has a user id '{1}' that is not found in the user table.",
                    activity.Id, userId));
                return null;
            }

            return new MemberJoinedActivity(activity, user);
        }

        /// <summary>
        /// Converts a paged list of generic activities into a paged list of more specific activity instances
        /// </summary>
        /// <param name="activities">Paged list of activities where each member may be a specific activity instance e.g. a profile updated activity</param>
        /// <param name="pageIndex"> </param>
        /// <param name="pageSize"> </param>
        /// <returns></returns>
        private PagedList<ActivityBase> ConvertToSpecificActivities(PagedList<Activity> activities, int pageIndex, int pageSize)
        {
            var listedResults = ConvertToSpecificActivities(activities);

            return new PagedList<ActivityBase>(listedResults, pageIndex, pageSize, activities.Count);
        }

        /// <summary>
        /// Converts a paged list of generic activities into a list of more specific activity instances
        /// </summary>
        /// <param name="activities"></param>
        /// <returns></returns>
        private IEnumerable<ActivityBase> ConvertToSpecificActivities(IEnumerable<Activity> activities)
        {
            var listedResults = new List<ActivityBase>();
            foreach (var activity in activities)
            {
                if (activity.Type == ActivityType.BadgeAwarded.ToString())
                {
                    var badgeActivity = GenerateBadgeActivity(activity);

                    if (badgeActivity != null)
                    {
                        listedResults.Add(badgeActivity);
                    }

                }
                else if (activity.Type == ActivityType.MemberJoined.ToString())
                {
                    var memberJoinedActivity = GenerateMemberJoinedActivity(activity);

                    if (memberJoinedActivity != null)
                    {
                        listedResults.Add(memberJoinedActivity);
                    }
                }
                else if (activity.Type == ActivityType.ProfileUpdated.ToString())
                {

                    var profileUpdatedActivity = GenerateProfileUpdatedActivity(activity);

                    if (profileUpdatedActivity != null)
                    {
                        listedResults.Add(profileUpdatedActivity);
                    }
                }
            }
            return listedResults;
        }

        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<ActivityBase> GetPagedGroupedActivities(int pageIndex, int pageSize)
        {
            // Read the database for all activities and convert each to a more specialised activity type

            var totalCount = ContextPerRequest.Db.Activity.AsNoTracking().Count();
            var results = ContextPerRequest.Db.Activity
                    .AsNoTracking()
                  .OrderByDescending(x => x.Timestamp)
                  .Skip((pageIndex - 1) * pageSize)
                  .Take(pageSize)
                  .ToList()
                  .OrderByDescending(x => x.Timestamp)
                  .ThenByDescending(x => x.Timestamp.TimeOfDay);
            
            var activities = new PagedList<Activity>(results, pageIndex, pageSize, totalCount);
            var specificActivities = ConvertToSpecificActivities(activities, pageIndex, pageSize);

            return specificActivities;
        }


        public PagedList<ActivityBase> SearchPagedGroupedActivities(string search, int pageIndex, int pageSize)
        {
            // Read the database for all activities and convert each to a more specialised activity type

            var totalCount = ContextPerRequest.Db.Activity.AsNoTracking().Count(x => x.Type.ToUpper().Contains(search.ToUpper()));
            // Get the topics using an efficient
            var results = ContextPerRequest.Db.Activity.AsNoTracking()
                  .Where(x => x.Type.ToUpper().Contains(search.ToUpper()))
                  .OrderByDescending(x => x.Timestamp)
                  .Skip((pageIndex - 1) * pageSize)
                  .Take(pageSize)
                  .ToList()
                  .OrderByDescending(x => x.Timestamp)
                  .ThenByDescending(x => x.Timestamp.TimeOfDay);


            var activities = new PagedList<Activity>(results, pageIndex, pageSize, totalCount);

            return ConvertToSpecificActivities(activities, pageIndex, pageSize); ;
        }

        public IEnumerable<ActivityBase> GetAll(int howMany)
        {
            var activities = ContextPerRequest.Db.Activity.AsNoTracking().Take(howMany);
            var specificActivities = ConvertToSpecificActivities(activities);
            return specificActivities;
        }

        /// <summary>
        /// Delete a list of activities
        /// </summary>
        /// <param name="activities"></param>
        public void Delete(IList<Activity> activities)
        {
            foreach (var activity in activities)
            {
                Delete(activity);
            }
        }
    }
}