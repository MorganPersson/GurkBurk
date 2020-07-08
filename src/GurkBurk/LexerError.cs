#if NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 ||NETCOREAPP1_0 || NETCOREAPP1_1
#define NO_SERIALIZE
#else
#define SERIALIZE
#endif

using System;
using GurkBurk.Internal;
#if SERIALIZE
using System.Runtime.Serialization;
#endif

namespace GurkBurk
{
#if SERIALIZE
    [Serializable]
#endif

    public class LexerError : Exception
    {
#if SERIALIZE
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
