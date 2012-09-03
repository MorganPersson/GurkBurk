using System.Collections.Generic;
using System.IO;

namespace GurkBurk.Internal
{
    public class UriLexer : RowLexer
    {
        public UriLexer(Lexer parent, LineEnumerator lineEnumerator, IListener listener, Language language)
            : base(parent, lineEnumerator, listener, language)
        { }

        public override IEnumerable<string> TokenWords
        {
            get { return new[] { @"file://", @"http://", @"https://" }; }
        }

        protected override bool CanSpanMultipleLines { get { return false; } }
        protected override void HandleToken(LineMatch match)
        {
            StreamReader reader = UriFactory.GetReader(match.Text);
            using (reader)
            {
                while (reader.EndOfStream == false)
                {
                    var text = reader.ReadLine().Trim(WhiteSpace);
                    if (!string.IsNullOrEmpty(text))
                        base.HandleToken(new LineMatch(match.Token, text, new ParsedLine(text, "\n", match.Line), this));
                }
            }
        }

        protected override IEnumerable<Lexer> Children
        {
            get { return new Lexer[0]; }
        }

        public override LineMatch Match(ParsedLine line)
        {
            var textLine = line.Text.Trim(WhiteSpace).ToLower();
            if (textLine.StartsWith("file://"))
                return new LineMatch(@"file://", line.Text, line, this);
            return null;
        }
    }
}