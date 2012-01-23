using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GurkBurk
{
    public class LineMatcher
    {
        private readonly Dictionary<string, Lexer> lexerLookup;
        private readonly Regex regex;

        public LineMatcher(IEnumerable<Lexer> lexers)
        {
            lexerLookup = BuildLexerLookUp(lexers);
            var words = lexers.Select(_ => new
                                               {
                                                   TokenWords = _.TokenWords.Select(TokenWordAsRegex(_)),
                                               })
                .SelectMany(_ => _.TokenWords)
                .Select(_ => new { TokenWord = _, Words = _.Where(c => c == ' ').Count() })
                .OrderByDescending(_ => _.Words)
                .Select(_ => _.TokenWord) //.Replace("|", @"\|"))
                .ToArray();
            string allWords = "(" + string.Join(")|(", words) + ")";
            regex = new Regex(string.Format(LineMatch, allWords));
        }

        private static Func<string, string> TokenWordAsRegex(Lexer lexer)
        {
            return t => t.Replace("|", @"\|") + (lexer.MustHaveSpaceOrKolonAfterToken ? @"(\s|:)" : "");
        }

        private const string LineMatch = @"\s*(?<keyword>{0})(?<text>.*)";

        public LineMatch Match(ParsedLine line)
        {
            var match = regex.Match(line.Text);
            if (match.Success == false)
                return null;
            var keyword = match.Groups["keyword"].Value.Trim().TrimEnd(new[] { ':' });
            var lexer = lexerLookup[keyword];
            return new LineMatch(keyword, match.Groups["text"].Value.Trim(), line, lexer);
        }

        private static Dictionary<string, Lexer> BuildLexerLookUp(IEnumerable<Lexer> lexers)
        {
            var d = new Dictionary<string, Lexer>();
            foreach (var lexer in lexers)
            {
                foreach (var tokenWord in lexer.TokenWords)
                {
                    d.Add(tokenWord, lexer);
                }
            }
            return d;
        }
    }
}