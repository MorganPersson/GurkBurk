using System;
using System.IO;

namespace GurkBurk.Internal
{
    public static class UriFactory
    {
        public static Func<Uri, StreamReader> fileReader = ReadFile;
        public static Func<Uri, StreamReader> httpReader = ReadHttp;

        public static StreamReader GetReader(string parsedLine)
        {
            var uri = new Uri(parsedLine);
            if (uri.IsFile)
                return fileReader(uri);
            return httpReader(uri);
        }

        public static void ResetToDefault()
        {
            fileReader = ReadFile;
            httpReader = ReadHttp;
        }

        private static StreamReader ReadFile(Uri uri)
        {
            return File.OpenText(uri.AbsolutePath);
        }

        private static StreamReader ReadHttp(Uri uri)
        {
            var request = new System.Net.WebClient();
            return new StreamReader(request.OpenRead(uri));
        }
    }
}