using System;
using System.Runtime.Serialization;
using GurkBurk.Internal;

namespace GurkBurk
{
    [Serializable]
    public class LexerError : Exception
    {
        public LexerError(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        {
        }

        public LexerError(ParsedLine currentWord)
            : base(string.Format("Error parsing line {0} on line '{1}'", currentWord.Line, currentWord.Text))
        {
        }
    }
}