using System;
using System.IO;

namespace GurkBurk.Internal
{
    public static class UriFactory
    {
        public static Func<Uri, StreamReader> fileReader = ReadFile;
#if !CF_35
        public static Func<Uri, StreamReader> httpReader = ReadHttp;
#endif
        public static StreamReader GetReader(string parsedLine)
        {
            var uri = new Uri(parsedLine);
            if (uri.IsFile)
                return fileReader(uri);
#if !CF_35
            return httpReader(uri);
#else
            throw new NotSupportedException("URL are not supported in Compact Framework");
#endif
        }

        public static void ResetToDefault()
        {
            fileReader = ReadFile;
#if !CF_35
            httpReader = ReadHttp;
#endif
        }

        private static StreamReader ReadFile(Uri uri)
        {
            return File.OpenText(uri.AbsolutePath);
        }
#if !CF_35
        private static StreamReader ReadHttp(Uri uri)
        {
            var request = new System.Net.WebClient();
            return new StreamReader(request.OpenRead(uri));
        }
#endif
    }
}