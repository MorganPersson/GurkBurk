using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GurkBurk.Internal
{
    public class CommentLexer : Lexer
    {
        private readonly Regex isLanguage = new Regex(@"language\s*(:|\s)\s*(?<language>[a-zA-Z\-]+)", RegexOptions.Compiled);
        private readonly Listener listener;

        public CommentLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
        }

        public override IEnumerable<string> TokenWords
        {
            get { return new[] {"#"}; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return new Lexer[0]; }
        }

        protected override bool CanSpanMultipleLines
        {
            get { return false; }
        }

        public override bool MustHaveSpaceOrKolonAfterToken
        {
            get { return false; }
        }

        protected override void HandleToken(LineMatch match)
        {
            string text = match.ParsedLine.Text;
            if (IsLanguageComment(text))
                ChangeLanguage(text);

            listener.comment(match.Text, match.Line);
        }

        private void ChangeLanguage(string comment)
        {
            if (isLanguage.IsMatch(comment))
            {
                var language = isLanguage.Match(comment).Groups["language"].Value;
                UseLanguage(language);
            }
        }

        private bool IsLanguageComment(string comment)
        {
            return isLanguage.IsMatch(comment);
        }
    }
}