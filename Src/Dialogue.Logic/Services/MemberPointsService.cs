using System;
using System.Collections.Generic;
using System.Linq;
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
            points.DateAdded = DateTime.UtcNow;
            ContextPerRequest.Db.MemberPoints.Add(points);
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

        public void Delete(MemberPoints item)
        {
            ContextPerRequest.Db.MemberPoints.Remove(item);
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

            var results = ContextPerRequest.Db.MemberPoints
                .Where(x => x.DateAdded >= start && x.DateAdded < end)
                .ToList();

            // Include Members
            PopulateAllObjects(results);

            return results.GroupBy(x => x.Member)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .OrderByDescending(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<Member, int> GetThisYearsPoints(int? amountToTake)
        {
            amountToTake = amountToTake ?? int.MaxValue;
            var thisYear = DateTime.UtcNow.Year;


            var results = ContextPerRequest.Db.MemberPoints
                .Where(x => x.DateAdded.Year == thisYear)
                .ToList();

            PopulateAllObjects(results);

            return results.GroupBy(x => x.Member)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .OrderByDescending(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<Member, int> GetAllTimePoints(int? amountToTake)
        {
            amountToTake = amountToTake ?? int.MaxValue;

            var results = ContextPerRequest.Db.MemberPoints
                .ToList();

            PopulateAllObjects(results);

            return results.GroupBy(x => x.Member)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .OrderByDescending(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<Member, int> GetAllTimePointsNegative(int? amountToTake)
        {
            amountToTake = amountToTake ?? int.MaxValue;

            var results = ContextPerRequest.Db.MemberPoints
                        .ToList();

            PopulateAllObjects(results);

            return results.GroupBy(x => x.Member)
                        .ToDictionary(x => x.Key, x => x.Select(p => p.Points).Sum())
                        .OrderBy(x => x.Value)
                        .Take((int)amountToTake)
                        .ToDictionary(x => x.Key, x => x.Value);
        }

        private static void PopulateAllObjects(List<MemberPoints> points)
        {
            if (points.Any())
            {
                var memberIds = points.Select(x => x.MemberId).Where(x => x != 0).ToList();
                var members = MemberMapper.MapMember(memberIds);
                foreach (var point in points)
                {
                    var member = members.FirstOrDefault(x => x.Id == point.MemberId);
                    point.Member = member;
                }
            }
        } 
    }
}