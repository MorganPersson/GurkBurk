using System.IO;

namespace GurkBurk.Internal
{
    public static class Extensions
    {
        public static TextReader ToTextReader(this string text)
        {
            var ms = new MemoryStream();
            var sr = new StreamWriter(ms);
            sr.Write(text);
            sr.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return new StreamReader(ms);
        }
    }
}