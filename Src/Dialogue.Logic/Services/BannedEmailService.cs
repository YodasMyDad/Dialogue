using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;

using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class BannedEmailService
    {

        public BannedEmail Add(BannedEmail bannedEmail)
        {
            return ContextPerRequest.Db.BannedEmail.Add(bannedEmail);
        }

        public void Delete(BannedEmail bannedEmail)
        {
            ContextPerRequest.Db.BannedEmail.Remove(bannedEmail);
        }

        public IEnumerable<BannedEmail> GetAll()
        {
            return ContextPerRequest.Db.BannedEmail.ToList();
        }

        public BannedEmail Get(int id)
        {
            return ContextPerRequest.Db.BannedEmail.FirstOrDefault(x => x.Id == id);
        }

        public PagedList<BannedEmail> GetAllPaged(int pageIndex, int pageSize)
        {
            var total = ContextPerRequest.Db.BannedEmail.Count();

            var results = ContextPerRequest.Db.BannedEmail
                                .OrderByDescending(x => x.Email)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedEmail>(results, pageIndex, pageSize, total);
        }

        public PagedList<BannedEmail> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            var total = ContextPerRequest.Db.BannedEmail.Count(x => x.Email.ToLower().Contains(search.ToLower()));

            var results = ContextPerRequest.Db.BannedEmail
                                .Where(x => x.Email.ToLower().Contains(search.ToLower()))
                                .OrderByDescending(x => x.Email)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedEmail>(results, pageIndex, pageSize, total);
        }

        public IEnumerable<BannedEmail> GetAllWildCards()
        {
            return ContextPerRequest.Db.BannedEmail.Where(x => x.Email.StartsWith("*@")).ToList();
        }

        public IEnumerable<BannedEmail> GetAllNonWildCards()
        {
            return ContextPerRequest.Db.BannedEmail.Where(x => !x.Email.StartsWith("*@")).ToList();
        }

        public bool EmailIsBanned(string email)
        {
            var domainBanned = false;


            // Split the email so we can get the domain out
            var emailDomain = ReturnDomainOnly(email).ToLower();

   
                // Now put them into two groups
                var wildCardEmails = GetAll().Where(x => x.Email.StartsWith("*@")).ToList();
                var nonWildCardEmails = GetAll().Except(wildCardEmails).ToList();

                if (wildCardEmails.Any())
                {
                    var wildCardDomains = wildCardEmails.Select(x => ReturnDomainOnly(x.Email));

                    // Firstly see if entire domain is banned
                    if (wildCardDomains.Any(domains => domains.ToLower() == emailDomain))
                    {
                        // Found so its banned
                        domainBanned = true;
                    }
                }

                // Domain is not banned so see if individual email is banned
                if (nonWildCardEmails.Any())
                {
                    if (nonWildCardEmails.Select(x => x.Email).Any(nonWildCardEmail => nonWildCardEmail.ToLower() == email))
                    {
                        domainBanned = true;
                    }
                }
       

            return domainBanned;
        }

        private static string ReturnDomainOnly(string email)
        {
            return email.Split('@')[1];
        }
    }
}