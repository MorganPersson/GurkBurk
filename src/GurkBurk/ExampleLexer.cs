using System.Collections.Generic;

namespace GurkBurk
{
    public class ExampleLexer : Lexer
    {
        private readonly Lexer[] children;

        public ExampleLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        {
            children = new Lexer[]
                           {
                               new RowLexer(this,lineEnumerator, listener,language), 
                               new CommentLexer(this,lineEnumerator, listener, language)
                           };
        }

        public override IEnumerable<string> TokenWords { get { return Language.Examples; } }
        protected override IEnumerable<Lexer> Children { get { return children; } }
        protected override bool CanSpanMultipleLines { get { return false; } }

        protected override void HandleToken(LineMatch match)
        {
            Listener.examples(match.Token, string.Empty, string.Empty, match.Line);
        }
    }
}