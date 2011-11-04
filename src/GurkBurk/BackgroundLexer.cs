using System.Collections.Generic;

namespace GurkBurk
{
    public class BackgroundLexer : Lexer
    {
        private readonly Lexer[] children;

        public BackgroundLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        {
            children = new Lexer[]
                         {
                             new CommentLexer(this, lineEnumerator, Listener, Language), 
                         };
        }

        public override IEnumerable<string> TokenWords { get { return Language.Background; } }
        protected override IEnumerable<Lexer> Children { get { return children; } }

        protected override void HandleToken(LineMatch match)
        {
            Listener.background(match.Token, match.Text, string.Empty, match.Line);
        }
    }
}