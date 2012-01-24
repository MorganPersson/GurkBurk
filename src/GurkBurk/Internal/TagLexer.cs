using System;
using System.Collections.Generic;
using System.Linq;

namespace GurkBurk.Internal
{
    public class TagLexer : Lexer
    {
        private readonly Listener listener;

        public TagLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
        }

        public override IEnumerable<string> TokenWords
        {
            get { return new[] {"@"}; }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return new Lexer[0]; }
        }

        protected override bool CanSpanMultipleLines
        {
            get { return false; }
        }

        public override bool MustHaveSpaceOrKolonAfterToken
        {
            get { return false; }
        }

        protected override void HandleToken(LineMatch match)
        {
            var tags = match.ParsedLine.Text
                .Split(new[] {'@'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => "@" + _.Trim());
            foreach (var tag in tags)
                listener.tag(tag, match.Line);
        }
    }
}