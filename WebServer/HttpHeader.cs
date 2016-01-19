using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    /// <summary>
    /// A class holding details of an HTTP Header
    /// </summary>
    public class HttpHeader
    {
        virtual public string Method { get; set; }
        virtual public string RequestURI { get; set; }
        public string HTTPVersionString { get; set; }
        //public Dictionary<string, string> HeaderFields { get; set; }
        public string NewLine { get; set; }

        public HttpHeaders Headers { get; set; }

        public HttpHeader()
        {
            //HeaderFields = new Dictionary<string, string>();
            Headers = new HttpHeaders();
            NewLine = "\r\n";
        }

        public class HttpHeaders
        {
            public HttpAcceptHeader Accept { get; set; }
            public string[] AcceptCharset { get; set; }
            public string[] AcceptEncoding { get; set; }
            public string[] AcceptLanguage { get; set; }
            public string Authorization { get; set; }
            public string Expect { get; set; }
            public string From { get; set; }
            public string Host { get; set; }
            public string IfMatch { get; set; }
            public string UserAgent { get; set; }
            public long? ContentLength { get; set; }
            public HttpCacheControlHeader CacheControl { get; set; }
            public HttpConnectionHeader Connection { get; set; }
            public string ContentType { get; set; }
            public DateTime? LastModified { get; set; }

            public HttpHeaders()
            {
                CacheControl = new HttpCacheControlHeader();
                Connection = new HttpConnectionHeader();
            }
        }

        /// <summary>
        /// Parse a string to make a HttpHeader
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static HttpHeader Parse(string header)
        {
            HttpHeader ret = new HttpHeader();

            // split into lines
            string[] lines = header.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // request line is first
            string[] parts = lines[0].Split(' ');
            ret.Method = parts[0];
            ret.RequestURI = parts[1];
            ret.HTTPVersionString = parts[2];

            // go through the rest of the strings and get data from them
            // other header fields are in 5.3 of https://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html
            for (int i = 1; i < lines.Length; i++)
            {
                // if the line doen't contain a colon then finish
                if (lines[i].IndexOf(':') < 0) break;

                // get header field name and value
                parts = lines[i].Split(':');
                string fieldName = parts[0];
                string value = parts[1].Trim();

                // add into the dictionary
                //ret.HeaderFields.Add(fieldName, value);

                // what is it? (case-insensitive https://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2)
                if (fieldName.Equals("Accept", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Accept = HttpAcceptHeader.Parse(value);
                }
                else if (fieldName.Equals("Accept-Charset", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.AcceptCharset = value.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (fieldName.Equals("Accept-Encoding", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.AcceptEncoding = value.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (fieldName.Equals("Accept-Language", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.AcceptLanguage = value.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (fieldName.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Authorization = value;
                }
                else if (fieldName.Equals("Expect", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Expect = value;
                }
                else if (fieldName.Equals("From", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.From = value;
                }
                else if (fieldName.Equals("Host", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Host = value;
                }
                else if (fieldName.Equals("If-Match", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.IfMatch = value;
                }
                else if (StrComp(fieldName, "Cache-Control"))
                {
                    ret.Headers.CacheControl = HttpCacheControlHeader.Parse(value);
                }
                else if (StrComp(fieldName, "User-Agent"))
                {
                    ret.Headers.UserAgent = value;
                }
                else if (StrComp(fieldName, "Connection"))
                {
                    ret.Headers.Connection = HttpConnectionHeader.Parse(value);
                }
                else if (StrComp(fieldName, "Content-Length"))
                {
                    ret.Headers.ContentLength = int.Parse(value);
                }
            }

            return ret;
        }


        private static bool StrComp(string one, string two)
        {
            return one.Equals(two, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
