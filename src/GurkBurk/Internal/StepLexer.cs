using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class StepLexer : Lexer
    {
        private readonly Lexer[] children;
        private readonly IListener listener;

        public StepLexer(Lexer parent, LineEnumerator lineEnumerator, IListener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
            children = new Lexer[]
                           {
                               new RowLexer(this, lineEnumerator, listener, language),
                               new CommentLexer(this, lineEnumerator, listener, language),
                               new DocStringLexer(this, lineEnumerator, listener, language),
                           };
        }

        public override IEnumerable<string> TokenWords
        {
            get { return Language.Steps; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return children; }
        }

        protected override void HandleToken(LineMatch match)
        {
            listener.Step(match.Token, match.Text, match.Line);
        }
    }
}