using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class ScenarioLexer : Lexer
    {
        private readonly Lexer[] children;
        protected readonly Listener Listener;

        public ScenarioLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            Listener = listener;

            children = new Lexer[]
                           {
                               new StepLexer(this, lineEnumerator, listener, language),
                               new ExampleLexer(this, lineEnumerator, listener, language),
                               new CommentLexer(this, lineEnumerator, listener, language)
                           };
        }

        public override IEnumerable<string> TokenWords
        {
            get { return Language.Scenario; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return children; }
        }

        protected override void HandleToken(LineMatch match)
        {
            Listener.scenario(match.Token, match.Text, string.Empty, match.Line);
        }
    }
}