using System.Collections.Generic;

namespace GurkBurk.Internal
{
    public class DocStringLexer : Lexer
    {
        private readonly Listener listener;

        public DocStringLexer(Lexer parent, LineEnumerator lineEnumerator, Listener listener, Language language)
            : base(parent, lineEnumerator, language)
        {
            this.listener = listener;
        }

        public override IEnumerable<string> TokenWords
        {
            get { return new[] { "\"\"\"" }; }
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

        private const string DocString = "\"\"\"";

        protected override void HandleToken(LineMatch match)
        {
            int line = match.Line;
            string text = match.Text;
            int spacesToTrim = match.ParsedLine.Text.IndexOf("\"\"\"", System.StringComparison.Ordinal);

            bool skipNewline = text == string.Empty;
            bool atEnd = text.Trim().Length > 3 && text.Trim().EndsWith(DocString);
            while (!atEnd && LineEnumerator.HasMore)
            {
                LineEnumerator.MoveToNext();
                var lineText = LineEnumerator.Current.Text;
                atEnd = lineText.TrimEnd().EndsWith(DocString);
                if (!atEnd)
                    text += (skipNewline ? TrimSpaces(lineText, spacesToTrim) : LineEnumerator.Current.LineEnd + TrimSpaces(lineText, spacesToTrim));
                skipNewline = false;
            }
            string trimEnd = text.TrimEnd(new[] { DocString[0] });
            listener.docString(trimEnd, line);
        }

        private string TrimSpaces(string text, int spacesToTrim)
        {
            var a = text.Substring(0, spacesToTrim);
            var b = text.Substring(spacesToTrim);
            if (a.Trim() == string.Empty)
                return b;
            return text.TrimStart();
        }
    }
}