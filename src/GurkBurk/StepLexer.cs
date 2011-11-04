using System.Collections.Generic;

namespace GurkBurk
{
    public class StepLexer : Lexer
    {
        private readonly Lexer[] children;

        public StepLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        {
            children = new Lexer[]
                           {
                               new RowLexer(this, lineEnumerator, listener, language), 
                               new CommentLexer(this, lineEnumerator, listener, language),
                               new DocStringLexer(this,lineEnumerator, listener,language), 
                           };
        }

        public override IEnumerable<string> TokenWords { get { return Language.Steps; } }
        protected override IEnumerable<Lexer> Children { get { return children; } }

        protected override void HandleToken(LineMatch match)
        {
            Listener.step(match.Token, match.Text, match.Line);
        }
    }
}