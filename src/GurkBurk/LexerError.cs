using System;

namespace GurkBurk
{
    public class LexerError : Exception
    {
        public LexerError(ParsedLine currentWord)
            : base(string.Format("Error parsing line {0} on line '{1}'", currentWord.Line, currentWord.Text))
        { }
    }
}