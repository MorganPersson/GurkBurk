using System;
using System.IO;
using System.Net.Http;

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

        private static Stream StringToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static StreamReader ReadHttp(Uri uri)
        {
            var response = "";
            using (var client = new HttpClient())
            {
                var httpResponse = client.GetAsync(uri).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseContent = httpResponse.Content;
                    response = responseContent.ReadAsStringAsync().Result;
                } else {
                    throw new Exception(string.Format("Failed with http statuscode {0} - {1}", httpResponse.StatusCode, httpResponse.ReasonPhrase));
                }
            }

        //     var request = new System.Net.WebClient();
            return new StreamReader(StringToStream(response));
        }
    }
}
