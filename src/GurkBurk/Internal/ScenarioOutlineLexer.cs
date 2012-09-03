using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class ScenarioOutlineLexer : ScenarioLexer
    {
        public ScenarioOutlineLexer(Lexer parent, LineEnumerator lineEnumerator, IListener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        {
        }

        public override IEnumerable<string> TokenWords
        {
            get { return Language.ScenarioOutline; }
        }

        protected override void HandleToken(LineMatch match)
        {
            Listener.ScenarioOutline(match.Token, match.Text, match.Line);
        }
    }
}