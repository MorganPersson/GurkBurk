using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class ExampleLexer : Lexer
    {
        private readonly Lexer[] children;
        private readonly IListener listener;

        public ExampleLexer(Lexer parent, LineEnumerator lineEnumerator, IListener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
            children = new Lexer[]
                           {
                               new RowLexer(this, lineEnumerator, listener, language),
                               new CommentLexer(this, lineEnumerator, listener, language),
                               new UriLexer(this, lineEnumerator, listener, language)
                           };
        }

        public override IEnumerable<string> TokenWords
        {
            get { return Language.Examples; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return children; }
        }

        protected override bool CanSpanMultipleLines
        {
            get { return false; }
        }

        protected override void HandleToken(LineMatch match)
        {
            listener.Examples(match.Token, string.Empty, match.Line);
        }
    }
}