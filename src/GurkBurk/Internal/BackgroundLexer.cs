using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class BackgroundLexer : Lexer
    {
        private readonly Lexer[] children;
        private readonly Listener listener;

        public BackgroundLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
            children = new Lexer[]
                           {
                               new CommentLexer(this, lineEnumerator, listener, language),
                               new StepLexer(this, lineEnumerator, listener, language),
                           };
        }

        public override IEnumerable<string> TokenWords
        {
            get { return Language.Background; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return children; }
        }

        protected override void HandleToken(LineMatch match)
        {
            listener.background(match.Token, match.Text, string.Empty, match.Line);
        }
    }
}