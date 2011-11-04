using System.Collections.Generic;

namespace GurkBurk
{
    public class DocStringLexer : Lexer
    {
        public DocStringLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        {
        }

        public override IEnumerable<string> TokenWords { get { return new[] { "\"\"\"" }; } }
        protected override IEnumerable<Lexer> Children { get { return new Lexer[0]; } }
        protected override bool CanSpanMultipleLines { get { return false; } }
        public override bool MustHaveSpaceOrKolonAfterToken { get { return false; } }

        const string DocString = "\"\"\"";
        protected override void HandleToken(LineMatch match)
        {
            int line = match.Line;
            string text = match.Text;
            bool atEnd = text.Trim().Length > 3 && text.Trim().EndsWith(DocString);
            while (!atEnd && LineEnumerator.HasMore)
            {
                LineEnumerator.MoveToNext();
                text += "\n" + LineEnumerator.Current.Text;
                atEnd = text.TrimEnd().EndsWith(DocString);
            }
            Listener.docString(text.TrimEnd(new[] { DocString[0] }), line);
        }
    }
}