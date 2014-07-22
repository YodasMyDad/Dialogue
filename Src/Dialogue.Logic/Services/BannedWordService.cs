using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class BannedWordService
    {

        public BannedWord Add(BannedWord bannedWord)
        {
            return ContextPerRequest.Db.BannedWord.Add(bannedWord);
        }

        public void Delete(BannedWord bannedWord)
        {
            ContextPerRequest.Db.BannedWord.Remove(bannedWord);
        }

        public IList<BannedWord> GetAll()
        {
            return ContextPerRequest.Db.BannedWord.OrderByDescending(x => x.DateAdded).ToList();
        }

        public BannedWord Get(int id)
        {
            return ContextPerRequest.Db.BannedWord.FirstOrDefault(x => x.Id == id);
        }

        public PagedList<BannedWord> GetAllPaged(int pageIndex, int pageSize)
        {
            var total = ContextPerRequest.Db.BannedWord.Count();

            var results = ContextPerRequest.Db.BannedWord
                                .OrderBy(x => x.Word)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedWord>(results, pageIndex, pageSize, total);
        }

        public PagedList<BannedWord> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            var total = ContextPerRequest.Db.BannedWord.Count(x => x.Word.ToLower().Contains(search.ToLower()));

            var results = ContextPerRequest.Db.BannedWord
                                .Where(x => x.Word.ToLower().Contains(search.ToLower()))
                                .OrderBy(x => x.Word)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedWord>(results, pageIndex, pageSize, total);
        }

        public string SanitiseBannedWords(string content)
        {
            var bannedWords = GetAll();
            if (bannedWords.Any())
            {
                return SanitiseBannedWords(content, bannedWords.Select(x => x.Word).ToList());
            }
            return content;
        }

        public string SanitiseBannedWords(string content, IList<string> words)
        {
            if (words != null && words.Any())
            {
                var censor = new CensorUtils(words);
                return censor.CensorText(content);
            }
            return content;
        }
    }
}