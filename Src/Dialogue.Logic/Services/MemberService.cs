using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Mapping;
using Dialogue.Logic.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Web.Security;
using Member = Dialogue.Logic.Models.Member;

namespace Dialogue.Logic.Services
{
    public partial class MemberService
    {
        private readonly Umbraco.Core.Services.IMemberService _memberService;
        private readonly Umbraco.Core.Services.IMemberGroupService _memberGroupService;
        private readonly MembershipHelper _membershipHelper;
        public MemberService()
        {
            _memberService = AppHelpers.UmbServices().MemberService;
            _memberGroupService = AppHelpers.UmbServices().MemberGroupService;
            _membershipHelper = AppHelpers.UmbMemberHelper();
        }

        #region Members

        public IList<Member> GetActiveMembers()
        {
            // Get members that last activity date is valid
            var date = DateTime.UtcNow.AddMinutes(-AppConstants.TimeSpanInMinutesToShowMembers);
            var ids = _memberService.GetMembersByPropertyValue("lastActiveDate", date, ValuePropertyMatchType.GreaterThan)
                .Where(x => x.IsApproved && !x.IsLockedOut)
                .Select(x => x.Id);
            return MemberMapper.MapMember(ids.ToList());
        }

        public Member Get(int id, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetById(id), populateFull);
        }

        public List<Member> GetUsersById(List<int> id, bool populateFull = false)
        {
            return MemberMapper.MapMember(id, populateFull);
        }

        public Member GetByEmail(string email, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetByEmail(email), populateFull);
        }

        public Member GetByUsername(string username, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetByUsername(username), populateFull);
        }

        public Member CurrentMember(bool populateFull = false)
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var key = string.Format("pub-memb-{0}-{1}", HttpContext.Current.User.Identity.Name, populateFull);
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, GetByUsername(HttpContext.Current.User.Identity.Name, populateFull));
                }
                return HttpContext.Current.Items[key] as Member;
            }

            return null;
        }

        public Member Get(string username, bool populateFull = false)
        {
            return MemberMapper.MapMember(_membershipHelper.GetByUsername(username), populateFull);
        }

        public MembersPaged GetAll(int pageIndex, int pageSize, out int totalRecords)
        {
            var members = _memberService.GetAll(pageIndex, pageSize, out totalRecords);
            var mappedMembers = MemberMapper.MapMember(members.Select(x => x.Id).ToList());
            return MemberMapper.MapPagedMember(mappedMembers, pageIndex, pageSize, totalRecords);
        }

        public List<Member> GetAllById(List<int> ids, bool populateFull = false)
        {
            var members = _memberService.GetAllMembers(ids.ToArray());
            return MemberMapper.MapMember(members.Select(x => x.Id).ToList());
        }

        public MembersPaged Search(int pageIndex, int pageSize, string searchTerm, out int totalRecords)
        {
            var members = _memberService.GetAll(pageIndex, pageSize, out totalRecords)
                            .Where(x => x.Username.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
            var mappedMembers = MemberMapper.MapMember(members.Select(x => x.Id).ToList());
            return MemberMapper.MapPagedMember(mappedMembers, pageIndex, pageSize, totalRecords);
        }

        public void Delete(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            _memberService.Delete(baseMember);
        }

        public void UpdateLastActiveDate(Member member)
        {
            var baseMember = _memberService.GetById(member.Id);
            baseMember.Properties[AppConstants.PropMemberLastActiveDate].Value = member.LastActiveDate;
            _memberService.Save(baseMember);
        }

        public int MemberCount()
        {
            return _memberService.GetMembersByMemberType(AppConstants.MemberTypeAlias).Count(x => x.IsApproved && !x.IsLockedOut);
        }

        public bool Login(string username, string password)
        {
            return AppHelpers.UmbMemberHelper().Login(username, password);
        }

        public IList<Member> GetLatestUsers(int amountToTake)
        {
            var ids = _memberService.GetMembersByMemberType(AppConstants.MemberTypeAlias)
              .OrderByDescending(x => x.CreateDate)
              .Take(amountToTake)
              .Select(x => x.Id);
            return MemberMapper.MapMember(ids.ToList());
        }

        public void LogOff()
        {
            AppHelpers.UmbMemberHelper().Logout();
        }

        public string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return AppHelpers.Lang("Members.Errors.DuplicateUserName");

                case MembershipCreateStatus.DuplicateEmail:
                    return AppHelpers.Lang("Members.Errors.DuplicateEmail");

                case MembershipCreateStatus.InvalidPassword:
                    return AppHelpers.Lang("Members.Errors.InvalidPassword");

                case MembershipCreateStatus.InvalidEmail:
                    return AppHelpers.Lang("Members.Errors.InvalidEmail");

                case MembershipCreateStatus.InvalidAnswer:
                    return AppHelpers.Lang("Members.Errors.InvalidAnswer");

                case MembershipCreateStatus.InvalidQuestion:
                    return AppHelpers.Lang("Members.Errors.InvalidQuestion");

                case MembershipCreateStatus.InvalidUserName:
                    return AppHelpers.Lang("Members.Errors.InvalidUserName");

                case MembershipCreateStatus.ProviderError:
                    return AppHelpers.Lang("Members.Errors.ProviderError");

                case MembershipCreateStatus.UserRejected:
                    return AppHelpers.Lang("Members.Errors.UserRejected");

                default:
                    return AppHelpers.Lang("Members.Errors.Unknown");
            }
        }

        public bool ResetPassword(Member member, string newPassword)
        {
            try
            {
                var iMember = _memberService.GetById(member.Id);
                _memberService.SavePassword(iMember, newPassword);
                return true;
            }
            catch (Exception ex)
            {
                AppHelpers.LogError("ResetPassword()", ex);
                return false;
            }

        } 
        #endregion

        #region Member Groups

        public IMemberGroup GetGroupByName(string groupName)
        {
            return _memberGroupService.GetByName(groupName);
        }

        public IEnumerable<IMemberGroup> GetAll()
        {
            return _memberGroupService.GetAll();
        }

        #endregion

    }
}