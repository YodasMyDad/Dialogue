using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class MemberPointsService
    {
        public IList<MemberPoints> GetByUser(Member user)
        {
            return GetByUser(user.Id);
        }

        public IList<MemberPoints> GetByUser(int memberId)
        {
            return ContextPerRequest.Db.MemberPoints
                    .Where(x => x.MemberId == memberId)
                    .ToList();
        }

        public MemberPoints Add(MemberPoints points)
        {
            if (points.MemberId <= 0)
            {
                AppHelpers.LogError("Error adding point memberId null");
            }
            else
            {
                points.DateAdded = DateTime.UtcNow;
                ContextPerRequest.Db.MemberPoints.Add(points);
            }
            return points;
        }

        public MemberPoints Get(Guid id)
        {
            return ContextPerRequest.Db.MemberPoints.FirstOrDefault(x => x.Id == id);
        }

        public List<MemberPoints> GetAllMemberPoints(int memberId)
        {
            return ContextPerRequest.Db.MemberPoints.Where(x => x.MemberId == memberId).ToList();
        }

        public void DeletePostPoints(Post post)
        {
            // Gets all points the member who made the post has gained from this post
            var membersPointsForThisPost =
                ContextPerRequest.Db.MemberPoints.Where(x => x.RelatedPostId == post.Id && x.MemberId == post.MemberId);

            // Now loop through and remove them
            foreach (var points in membersPointsForThisPost)
            {
                Delete(points);
            }
        }

        public void Delete(MemberPoints item)
        {
            if (item != null)
            {
                ContextPerRequest.Db.MemberPoints.Remove(item);   
            }
        }

        public void Delete(int amount, Member user)
        {
            var points = ContextPerRequest.Db.MemberPoints.FirstOrDefault(x => x.Points == amount && x.MemberId == user.Id);
            Delete(points);
        }

        public Dictionary<Member, int> GetCurrentWeeksPoints(int? amountToTake)
        {
            amountToTake = amountToTake ?? int.MaxValue;
            var date = DateTime.UtcNow;
            var start = date.Date.AddDays(-(int)date.DayOfWeek);
            var end = start.AddDays(7);

            var results = ContextPerRequest.Db.MemberPoints.AsNoTracking()
                .Where(x => x.DateAdded >= start && x.DateAdded < end);

            var memberIds = results
                        .Include(x => x.Points)
                        .GroupBy(x => x.MemberId)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .OrderByDescending(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);

            return PopulateAllObjects(memberIds);
        }

        public Dictionary<Member, int> GetThisYearsPoints(int? amountToTake)
        {
            amountToTake = amountToTake ?? int.MaxValue;
            var thisYear = DateTime.UtcNow.Year;


            var results = ContextPerRequest.Db.MemberPoints.AsNoTracking()
                .Where(x => x.DateAdded.Year == thisYear);

            var memberIds = results
                        .Include(x => x.Points).AsNoTracking()
                        .GroupBy(x => x.MemberId)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .OrderByDescending(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);

            return PopulateAllObjects(memberIds);
        }

        public Dictionary<Member, int> GetLatestNegativeUsers(int? amountToTake)
        {
            amountToTake = amountToTake ?? int.MaxValue;

            var thirtyDays = DateTime.Now.AddDays(-30);
            var results = ContextPerRequest.Db.MemberPoints.AsNoTracking()
                .Where(x => x.DateAdded > thirtyDays);

            var memberIds = results
                        .Include(x => x.Points)
                        .GroupBy(x => x.MemberId)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .Where(x => x.Value < 0)
                        .OrderBy(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);

            return PopulateAllObjects(memberIds);
        }

        public Dictionary<Member, int> GetAllTimePointsNegative(int? amountToTake)
        {
            amountToTake = amountToTake ?? int.MaxValue;

            var results = ContextPerRequest.Db.MemberPoints.AsNoTracking().Where(x => x.Points < 0).Include(x => x.Points);

            var memberIds = results.GroupBy(x => x.MemberId)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .OrderBy(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);

            return PopulateAllObjects(memberIds);
        }

        private static Dictionary<Member, int> PopulateAllObjects(Dictionary<int, int> groupedDict)
        {
            if (groupedDict.Any())
            {
                var returnDict = new Dictionary<Member, int>();
                var memberIds = groupedDict.Select(x => x.Key).Where(x => x != 0).ToList();
                var members = MemberMapper.MapMember(memberIds);
                foreach (var dict in groupedDict)
                {
                    var member = members.FirstOrDefault(x => x.Id == dict.Key);
                    returnDict.Add(member, dict.Value);
                }
                return returnDict;
            }
            return new Dictionary<Member, int>();
        } 
    }
}
