using System;
using System.Collections.Generic;

namespace GurkBurk
{
    public class TagLexer : Lexer
    {
        public TagLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        {
        }

        public override IEnumerable<string> TokenWords { get { return new[] { "@" }; } }
        protected override IEnumerable<Lexer> Children { get { return new Lexer[0]; } }
        protected override bool CanSpanMultipleLines { get { return false; } }
        public override bool MustHaveSpaceOrKolonAfterToken { get { return false; } }

        protected override void HandleToken(LineMatch match)
        {
            var tags = match.Text.Split(new[] { ' ', '@' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
                Listener.tag(tag, match.Line);
        }
    }
}