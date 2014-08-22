using System.Collections.Generic;
using System.Linq;

namespace Dialogue.Logic.Services
{
    public partial class BannedEmailService
    {
        public IEnumerable<string> GetAllWildCards()
        {
            return Dialogue.Settings().BannedEmails.Where(x => x.StartsWith("*@")).ToList();
        }

        public IEnumerable<string> GetAllNonWildCards()
        {
            return Dialogue.Settings().BannedEmails.Where(x => !x.StartsWith("*@")).ToList();
        }

        public bool EmailIsBanned(string email)
        {
            var domainBanned = false;

            // Split the email so we can get the domain out
            var emailDomain = ReturnDomainOnly(email).ToLower();

   
                // Now put them into two groups
                var wildCardEmails = Dialogue.Settings().BannedEmails.Where(x => x.StartsWith("*@")).ToList();
                var nonWildCardEmails = Dialogue.Settings().BannedEmails.Except(wildCardEmails).ToList();

                if (wildCardEmails.Any())
                {
                    var wildCardDomains = wildCardEmails.Select(ReturnDomainOnly);

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
                    if (nonWildCardEmails.Any(nonWildCardEmail => nonWildCardEmail.ToLower() == email))
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