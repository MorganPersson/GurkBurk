using System.Collections.Generic;
using System.Linq;

namespace GurkBurk.Internal
{
    public class LineMatcher
    {
        private class LexerForTokenWord
        {
            public Lexer Lexer { get; private set; }
            public string TokenWord { get; private set; }

            public LexerForTokenWord(Lexer lexer, string tokenWord)
            {
                Lexer = lexer;
                TokenWord = tokenWord;
            }
        }

        private readonly List<LexerForTokenWord> lexers;

        public LineMatcher(IEnumerable<Lexer> lexers)
        {
            this.lexers = lexers.SelectMany(l => l.TokenWords, (l, t) => new LexerForTokenWord(l, t)).OrderByDescending(_ => _.TokenWord.Length).ToList();
        }

        public LineMatch Match(ParsedLine line)
        {
            LineMatch match = null;
            lexers.FirstOrDefault(_ =>
                                      {
                                          match = _.Lexer.Match(line);
                                          return match != null;
                                      });
            return match;
        }
    }
}