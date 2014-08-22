using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;

namespace Dialogue.Logic.Services
{
    public partial class BannedWordService
    {

        public string SanitiseBannedWords(string content)
        {
            var bannedWords = Dialogue.Settings().BannedWords;
            if (bannedWords.Any())
            {
                return SanitiseBannedWords(content, bannedWords);
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