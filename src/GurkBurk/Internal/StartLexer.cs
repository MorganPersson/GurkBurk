using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class StartLexer : Lexer
    {
        private readonly Lexer[] children;

        public StartLexer(Lexer parent, LineEnumerator lineEnumerator, IListener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            children = new Lexer[]
                           {
                               new FeatureLexer(this, lineEnumerator, listener, language),
                               new TagLexer(this, lineEnumerator, listener, language),
                               new CommentLexer(this, lineEnumerator, listener, language),
                               new ScenarioLexer(this, lineEnumerator, listener, language),
                           };
        }

        public override IEnumerable<string> TokenWords
        {
            get { return new string[0]; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return children; }
        }

        protected override bool CanSpanMultipleLines
        {
            get { return false; }
        }

        protected override void HandleToken(LineMatch ignore)
        {
        }
    }
}