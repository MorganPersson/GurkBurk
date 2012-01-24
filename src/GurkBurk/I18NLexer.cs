using System.IO;
using GurkBurk.Internal;

namespace GurkBurk
{
    public class I18nLexer
    {
        private readonly Listener listener;

        public I18nLexer(Listener listener)
        {
            this.listener = listener;
        }

        public void scan(string text)
        {
            scan(text.ToTextReader());
        }

        public void scan(TextReader toTextReader)
        {
            var lineEnumerator = new LineEnumerator(toTextReader);
            Lexer s = new StartLexer(null, lineEnumerator, listener, new Language());
            lineEnumerator.MoveToNext();
            s.Parse();
            if ((lineEnumerator.HasMore || (string.IsNullOrEmpty(lineEnumerator.Current.Text) == false)))
                throw new LexerError(new ParsedLine(lineEnumerator.Current.Text, lineEnumerator.Current.Line));
            listener.eof();
        }
    }
}