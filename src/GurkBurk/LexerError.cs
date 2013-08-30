using System;
using System.Runtime.Serialization;
using GurkBurk.Internal;

namespace GurkBurk
{
    [Serializable]
    public class LexerError : Exception
    {
#if !CF_35
        public LexerError(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        { }
#endif
        public LexerError(ParsedLine currentWord)
            : base(string.Format("Line: {0}. Failed to parse '{1}'", currentWord.Line, currentWord.Text))
        { }

        public LexerError(string message)
            : base(message)
        { }
    }
}