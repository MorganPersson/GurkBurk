using System.Collections.Generic;

namespace GurkBurk
{
    public class ScenarioOutlineLexer : ScenarioLexer
    {
        public ScenarioOutlineLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        {
        }

        public override IEnumerable<string> TokenWords { get { return Language.ScenarioOutline; } }

        protected override void HandleToken(LineMatch match)
        {
            Listener.scenarioOutline(match.Token, match.Text, string.Empty, match.Line);
        }
    }
}