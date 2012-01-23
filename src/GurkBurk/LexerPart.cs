using System;
using System.Collections.Generic;
using System.Linq;

namespace GurkBurk
{
    public class LexerPart
    {
        private readonly Func<Language, IEnumerable<string>> tokenWords;
        private readonly List<LexerPart> children = new List<LexerPart>();
        private readonly List<Lexer> lexerChildren = new List<Lexer>();
        private readonly Action<LineMatch> handleToken;
        private readonly Language language;

        public LexerPart(Func<Language, IEnumerable<string>> tokenWords, Action<LineMatch> handleToken, Language language)
        {
            this.tokenWords = tokenWords;
            this.handleToken = handleToken;
            this.language = language;
        }

        public Action<LineMatch> HandleToken { get { return handleToken; } }
        public IEnumerable<Lexer> Children
        {
            get
            {
                if (lexerChildren.Any())
                    return lexerChildren;
                foreach (var child in children)
                    lexerChildren.Add(child.Clone().Build());
                return lexerChildren;
            }
        }

        private Lexer Build()
        {
            return null;
        }

        private LexerPart Clone()
        {
            var lexerPart = new LexerPart(tokenWords, handleToken, language);
            lexerPart.children.AddRange(children.Select(_ => _.Clone()));
            return lexerPart;
        }

        public IEnumerable<string> TokenWords()
        {
            return tokenWords(language);
        }

        public void AddChildren(params LexerPart[] children)
        {
            foreach (var child in children)
                this.children.Add(child);
        }
    }
}