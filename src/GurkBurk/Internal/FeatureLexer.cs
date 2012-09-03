using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class FeatureLexer : Lexer
    {
        private readonly Lexer[] children;
        private readonly IListener listener;

        public FeatureLexer(Lexer parent, LineEnumerator lineEnumerator, IListener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
            children = new Lexer[]
                           {
                               new ScenarioLexer(this, lineEnumerator, listener, language),
                               new CommentLexer(this, lineEnumerator, listener, language),
                               new ScenarioOutlineLexer(this, lineEnumerator, listener, language),
                               new BackgroundLexer(this, lineEnumerator, listener, language),
                               new TagLexer(this, lineEnumerator, listener, language),
                           };
        }

        public override IEnumerable<string> TokenWords
        {
            get { return Language.Feature; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return children; }
        }

        protected override void HandleToken(LineMatch match)
        {
            var newLine = match.Text.IndexOf("\n");
            string title = (newLine < 0) ? match.Text : match.Text.Substring(0, newLine);
            title = title.TrimEnd(new[] {'\r', ' ', '\t'});
            string narrative = (newLine < 0) ? "" : match.Text.Substring(newLine).TrimStart(new[] {'\n'});
            listener.Feature(match.Token, title, narrative, match.Line);
        }
    }
}